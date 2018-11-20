using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WT_Wiki_Bot_in_CSharp {

    internal class Blk
    {
        // Dictionary of C++ Types.
        private static readonly Dictionary<int, string> TypeList = new Dictionary<int, string> {
            {0x0, "size"}, {0x1, "str"}, {0x2, "int"}, {0x3, "float"}, {0x4, "vec2f"},
            {0x5, "vec3f"}, {0x6, "vec4f"}, {0x7, "vec2i"}, {0x8, "typex8"}, {0x9, "bool"},
            {0xa, "color"}, {0xb, "m4x3f"}, {0xc, "time"}, {0x10, "typex7"}, {0x89, "typex"}
        };

        /*
        // In game names for types
        var typeListStrict = new Dictionary<int, string>
        {
            {0x0, "size"}, {0x1, "t"}, {0x2, "i"}, {0x3, "r"}, {0x4, "p2"},
            {0x5, "p3"}, {0x6, "p4"}, {0x7, "ip2"}, {0x8, "typex8"}, {0x9, "b"},
            {0xa, "c"}, {0xb, "m"}, {0xc, "i64"}, {0x10, "typex7"}, {0x89, "b"}
        };
        */

        /// <summary>
        /// In-house version of War Thunder Blk File Unpacker.
        /// </summary>
        /// <remarks>
        /// Converted from Python of https://github.com/klensy/wt-tools/tree/dev
        /// </remarks>
        public static void BlkUnpack(FileInfo fileStuff) {
            // Various Constants I have no clue where they came from.
            const int numOfUnitsInFileCount = 0xe;
            const int unitsLengthTypeCount = 0xd;
            // Contains Found Keys
            var keyList = new Dictionary<int, string>();

            // Starting File Read
            using (var fileContents = new BinaryReader(fileStuff.Open(FileMode.Open))) {
                FileTypeErr(fileContents);

                // FileTypeErr already read three bytes in. Fourth Byte is version number.
                // As of November 16, 2018, it's Version 3.
                if (fileContents.ReadByte() != 3) {
                    throw new Exception("Outdated File Version.");
                }

                // Decimal-hexadecimal-binary conversion table
                // https://kb.iu.edu/d/afdl

                // Fucking Magic.
                fileContents.BaseStream.Seek(unitsLengthTypeCount, 0);
                int numOfUnitsInFile, curPos;
                switch (fileContents.ReadByte()) {
                    case 65: // 0x41
                        curPos = 15;
                        fileContents.BaseStream.Seek(numOfUnitsInFileCount, 0);
                        numOfUnitsInFile = fileContents.ReadByte();
                        break;

                    case 129: // 0x81
                        curPos = 16;
                        fileContents.BaseStream.Seek(numOfUnitsInFileCount, 0);
                        numOfUnitsInFile = fileContents.ReadUInt16();
                        break;

                    default:
                        throw new Exception("Unknown numOfUnitsInFile!");
                }

                fileContents.BaseStream.Seek(curPos, 0);
                for (var i = 0; i < numOfUnitsInFile; i++) {
                    // Format: <Variable Length><Variable Key>
                    var unitLength = fileContents.ReadByte();
                    var keyName = new string(Encoding.UTF8.GetChars(fileContents.ReadBytes(unitLength)));
                    // Adding keyName to Key List
                    // I need to find out how Prev Author found this hash.
                    var keyHash = 5;
                    foreach (var c in keyName) {
                        keyHash = (33 * keyHash + c) & 0xff;
                    }
                    while (keyList.ContainsKey(keyHash)) {
                        keyHash += 0x100;
                    }
                    keyList.Add(keyHash, keyName);
                }

                // Align to 4
                while (fileContents.BaseStream.Position % 4 != 0) {
                    // Shifting forward until base 4.
                    fileContents.BaseStream.Seek(1, SeekOrigin.Current);
                }

                // Test if there exist sub_units_names block
                int totalSubUnits;
                if (fileContents.ReadUInt16() > 0) {
                    // Shift forward one.
                    fileContents.BaseStream.Seek(1, SeekOrigin.Current);
                    switch (fileContents.ReadByte()) {
                        case 64: // 0x40
                            totalSubUnits = fileContents.ReadByte();
                            break;

                        case 128: // 0x80
                            totalSubUnits = fileContents.ReadUInt16();
                            break;

                        default:
                            throw new Exception("Unknown subBlockUnitType!");
                    }
                } else {
                    totalSubUnits = 4;
                    // Shift Forward 4
                    fileContents.BaseStream.Seek(4, SeekOrigin.Current);
                }

                // Finding subunit keys
                var subUnitKeys = new List<byte[]>();
                for (var subUnit = 0; subUnit < totalSubUnits; subUnit++) {
                    var unitLength = fileContents.ReadByte();
                    if (unitLength >= 0x80) {
                        unitLength = (byte)((unitLength - 0x80) * 0x100 + fileContents.ReadByte());
                    }
                    subUnitKeys.Add(fileContents.ReadBytes(unitLength));
                }
                // Align to 4
                while (fileContents.BaseStream.Position % 4 != 0) {
                    fileContents.BaseStream.Seek(1, SeekOrigin.Current);
                }

                var parsedBlock = ParseData(subUnitKeys, fileContents, keyList);
                Console.WriteLine("Test");
            }
        }

        /// <summary>
        /// Unpacks inner blocks.
        /// </summary>
        private static Dictionary<string, object> ParseData(IReadOnlyList<byte[]> subUnitKeys, BinaryReader fileReader, IReadOnlyDictionary<int, string> keyList) {
            var curBlock = new Dictionary<string, object>();
            var blockSize = new[] { fileReader.ReadUInt16(), fileReader.ReadUInt16() };
            while (fileReader.PeekChar() != -1) {
                if (blockSize[0] > 0) {
                    /*
                     * blockKeyList:
                     * [0]. Block ID
                     * [1]. Block Type
                     * [2]. Block Value
                     */
                    var blockKeyList = new List<object[]>();
                    for (var i = 0; i < blockSize[0]; i++) {
                        var cP = fileReader.BaseStream.Position;
                        var keyInfo = GetBlockInfo(fileReader);
                        blockKeyList.Add(keyInfo);
                        // Move to next 4 block.
                        fileReader.BaseStream.Seek(cP + 4, SeekOrigin.Begin);
                    }

                    foreach (var objects in blockKeyList) {
                        // If not boolean or typex
                        var finalObj = objects;
                        if ((byte)finalObj[1] != 0x9 && (byte)finalObj[1] != 0x89) {
                            finalObj[2] = GetBlockValue((byte)finalObj[1], fileReader);
                        }
                        curBlock = CheckBlock(FromIDtoString(finalObj, subUnitKeys, keyList), curBlock);
                    }

                    blockSize[0] = 0;
                } else {
                    // For blockSize[0] == 0
                    var keyInfo = GetBlockInfo(fileReader);
                    if (keyInfo[2] != null) {
                        fileReader.BaseStream.Seek(-4, SeekOrigin.Current);
                        var innerBlock = ParseData(subUnitKeys, fileReader, keyList);
                        var newInfo = new[] {
                            keyInfo[0],
                            keyInfo[1],
                            innerBlock
                        };
                        curBlock = CheckBlock(FromIDtoString(newInfo, subUnitKeys, keyList), curBlock);
                    }
                }

                if (blockSize.SequenceEqual(new ushort[] { 0, 0 })) {
                    break;
                }
            }

            return curBlock;
        }

        private static object[] GetBlockInfo(BinaryReader fileReader) {
            var keyInfo = new object[3];
            keyInfo[0] = fileReader.ReadUInt16();
            // Skipping one.
            fileReader.BaseStream.Seek(1, SeekOrigin.Current);
            var keyType = fileReader.ReadByte();
            keyInfo[1] = keyType;
            keyInfo[2] = GetBlockValue(keyType, fileReader);
            return keyInfo;
        }

        /// <summary>
        /// Checks for duplication in curBlock before appending to curBlock.
        /// </summary>
        private static int _usedPlaceHolders = 0;

        private static Dictionary<string, object> CheckBlock(IReadOnlyList<object> dataObjects, Dictionary<string, object> curBlock) {
            if (curBlock.ContainsKey((string)dataObjects[0])) {
                var newKey = (string)dataObjects[0] + _usedPlaceHolders++;
                curBlock.Add(newKey, dataObjects[1]);
            }
            curBlock.Add((string)dataObjects[0], dataObjects[1]);
            return curBlock;
        }

        /// <summary>
        /// Constructs return object.
        /// </summary>
        private static object[] FromIDtoString(IReadOnlyList<object> blockInfo, IReadOnlyList<byte[]> subUnitKeys, IReadOnlyDictionary<int, string> keyList) {
            var dataObj = new object[2];
            dataObj[0] = keyList[(ushort)blockInfo[0]];
            // If bType is not 'size'
            if ((byte)blockInfo[1] != 0x0) {
                dataObj[1] = ConvertInfo(blockInfo, subUnitKeys);
            }
            return dataObj;
        }

        /// <summary>
        /// Returns usable data.
        /// </summary>
        private static dynamic ConvertInfo(IReadOnlyList<dynamic> blockInfo, IReadOnlyList<byte[]> subUnitKeys) {
            switch (TypeList[(byte)blockInfo[1]]) {
                case "str":
                    var s = subUnitKeys[(byte)blockInfo[2]];
                    try {
                        return System.Text.Encoding.UTF8.GetString(s);
                    } catch (Exception e) {
                        Console.WriteLine(e);
                        throw;
                    }
                case "float":
                    return (decimal)blockInfo[2];

                case "color":
                    return $"#{blockInfo[2]:8x}";

                case "typex7":
                case "typex":
                case "int":
                case "bool":
                case "vec2i":
                case "vec2f":
                case "vec3f":
                case "vec4f":
                case "typex8":
                case "m4x3f":
                    return blockInfo[2];

                case "time":
                    return blockInfo[2][0];

                default:
                    throw new Exception("Type not found in Conversion!");
            }
        }

        /// <summary>
        /// Uses TypeList to identify then return block value.
        /// </summary>
        private static dynamic GetBlockValue(int blockType, BinaryReader fileReader) {
            if (!TypeList.ContainsKey(blockType)) {
                throw new Exception("TypeList does not contain blockType!");
            }

            switch (TypeList[blockType]) {
                case "str":
                    return fileReader.ReadUInt32();

                case "int":
                    return fileReader.ReadInt32();

                case "float":
                    return fileReader.ReadSingle();

                case "typex":
                    fileReader.BaseStream.Seek(2, SeekOrigin.Current);
                    // Reversed Boolean
                    return !fileReader.ReadBoolean();

                case "bool":
                    fileReader.BaseStream.Seek(2, SeekOrigin.Current);
                    return fileReader.ReadBoolean();

                case "size":
                    ushort[] pShort =
                    {
                        fileReader.ReadUInt16(),
                        fileReader.ReadUInt16()
                    };
                    return pShort;

                case "vec2f":
                    float[] pFloat1 =
                    {
                        fileReader.ReadSingle(),
                        fileReader.ReadSingle()
                    };
                    return pFloat1;

                case "vec3f":
                    float[] pFloat2 =
                    {
                        fileReader.ReadSingle(),
                        fileReader.ReadSingle(),
                        fileReader.ReadSingle()
                    };
                    return pFloat2;

                case "vec2i":
                    uint[] pInt =
                    {
                        fileReader.ReadUInt32(),
                        fileReader.ReadUInt32()
                    };
                    return pInt;

                case "time":
                    uint[] pTime =
                    {
                        fileReader.ReadUInt32(),
                        fileReader.ReadUInt32()
                    };
                    return pTime;

                case "vec4f":
                    float[] pFloat3 =
                    {
                        fileReader.ReadSingle(),
                        fileReader.ReadSingle(),
                        fileReader.ReadSingle(),
                        fileReader.ReadSingle()
                    };
                    return pFloat3;

                case "m4x3f":
                    float[] ret =
                    {
                        GetBlockValue(0x5, fileReader),
                        GetBlockValue(0x5, fileReader),
                        GetBlockValue(0x5, fileReader),
                        GetBlockValue(0x5, fileReader)
                    };
                    return ret;

                case "color":
                    return fileReader.ReadUInt32();

                case "typex7":
                    return fileReader.ReadUInt32();

                case "typex8":
                    uint[] pT8 =
                    {
                        fileReader.ReadUInt32(),
                        fileReader.ReadUInt32(),
                        fileReader.ReadUInt32()
                    };
                    return pT8;

                default:
                    throw new Exception("Switch Loop Died!");
            }
        }

        /// <summary>
        /// Checking File Header. BLK: "\x0BBF" or "00424246" in Hex.
        /// Throws Exception with Filename as parameter.
        /// </summary>
        private static void FileTypeErr(BinaryReader fileInput) {
            // Todo: Add File Name to Exception.
            if (ByteArrayToString(fileInput.ReadBytes(4)) != "00424246") {
                throw new Exception($"Error: File is not a proper .blk file.");
            }
        }

        /// <summary>
        /// Converts bytes to hex string.
        /// </summary>
        /// <remarks>
        /// https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa
        /// </remarks>
        private static string ByteArrayToString(byte[] arrBytes) {
            var hex = BitConverter.ToString(arrBytes);
            return hex.Replace("-", "");
        }
    }
}