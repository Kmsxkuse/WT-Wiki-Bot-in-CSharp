using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WT_Wiki_Bot_in_CSharp
{
    internal class Blk
    {   
        // Dictionary of C++ Types.
        private static readonly Dictionary<int, string> TypeList = new Dictionary<int, string>
        {
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
        public static void BlkUnpack(FileInfo fileStuff)
        {
            // Various Constants I have no clue where they came from.
            const int numOfUnitsInFileCount = 0xe;
            const int unitsLengthTypeCount = 0xd;
            // Contains Found Keys
            var keyList = new Dictionary<int, string>();

            // Starting File Read
            using (var fileContents = new BinaryReader(fileStuff.Open(FileMode.Open)))
            {
                FileTypeErr(fileContents);

                // FileTypeErr already read three bytes in. Fourth Byte is version number.
                // As of November 16, 2018, it's Version 3.
                if (fileContents.ReadByte() != 3)
                    throw new Exception("Outdated File Version.");

                // Decimal-hexadecimal-binary conversion table
                // https://kb.iu.edu/d/afdl

                // Fucking Magic.
                fileContents.BaseStream.Seek(unitsLengthTypeCount, 0);
                int numOfUnitsInFile, curPos;
                fileContents.BaseStream.Seek(numOfUnitsInFileCount, 0);
                switch (fileContents.ReadByte())
                {
                    case 65:
                        curPos = 15;
                        numOfUnitsInFile = fileContents.ReadByte();
                        break;
                    case 129:
                        curPos = 16;
                        numOfUnitsInFile = fileContents.ReadUInt16();
                        break;
                    default:
                        throw new Exception("Unknown numOfUnitsInFile!");
                }

                fileContents.BaseStream.Seek(curPos, 0);
                for (var i = 0; i < numOfUnitsInFile; i++)
                {
                    // Format: <Variable Length><Variable Key>
                    var unitLength = fileContents.ReadByte();
                    var keyName = new string(Encoding.UTF8.GetChars(fileContents.ReadBytes(unitLength)));
                    // Adding keyName to Key List
                    keyList.Add(keyList.Count, keyName);
                }

                // Align to 4
                while (fileContents.BaseStream.Position % 4 != 0)
                {
                    // Shifting forward until base 4.
                    fileContents.BaseStream.Seek(1, SeekOrigin.Current);
                }
                // Test if there exist sub_units_names block
                int totalSubUnits;
                if (fileContents.ReadUInt16() > 0)
                {
                    // Shift forward one.
                    fileContents.BaseStream.Seek(1, SeekOrigin.Current);
                    switch (fileContents.ReadByte())
                    {
                        case 64:
                            totalSubUnits = fileContents.ReadByte();
                            break;
                        case 128:
                            totalSubUnits = fileContents.ReadUInt16();
                            break;
                        default:
                            throw new Exception("Unknown subBlockUnitType!");
                    }
                }
                else
                {
                    totalSubUnits = 4;
                    // Shift Forward 4
                    fileContents.BaseStream.Seek(4, SeekOrigin.Current);
                }

                // Finding subunit keys
                var subUnitKeys = new List<string>();
                for (var subUnit = 0; subUnit < totalSubUnits; subUnit++)
                {
                    var unitLength = fileContents.ReadByte();
                    if (unitLength >= 0x80)
                    {
                        unitLength = (byte)((unitLength - 0x80) * 0x100 + fileContents.ReadByte());
                    }
                    subUnitKeys.Add(new string(fileContents.ReadChars(unitLength)));
                }
                // Align to 4
                while (fileContents.BaseStream.Position % 4 != 0)
                {
                    fileContents.BaseStream.Seek(1, SeekOrigin.Current);
                }

                ParseData(subUnitKeys, fileContents);
                Console.WriteLine("Test");
            }
        }

        /// <summary>
        /// Unpacks inner blocks.
        /// </summary>
        private static void ParseData(List<string> subUnitKeys, BinaryReader fileReader)
        {
            var blockSize = fileReader.ReadUInt16();
            // Dump next two.
            fileReader.BaseStream.Seek(2, SeekOrigin.Current);
            var curBlock = new Dictionary<string, object>();
            while (fileReader.PeekChar() != -1)
            {
                if (blockSize > 0)
                {
                    var blockKeyList = new List<object[]>();
                    for (var i = 0; i < blockSize; i++)
                    {
                        var keyInfo = new object[3];
                        var cP = fileReader.BaseStream.Position;
                        keyInfo[0] = fileReader.ReadUInt16();
                        // Skipping one.
                        fileReader.BaseStream.Seek(1, SeekOrigin.Current);
                        var keyId = fileReader.ReadByte();
                        keyInfo[1] = keyId;
                        keyInfo[2] = GetBlockValue(keyId, fileReader);
                        blockKeyList.Add(keyInfo);
                        fileReader.BaseStream.Seek(cP + 4, SeekOrigin.Begin);
                    }
                    Console.WriteLine("Test");
                }
                else
                {

                }
            }
        }
        /// <summary>
        /// Uses TypeList to identify then return block value.
        /// </summary>
        private static dynamic GetBlockValue(int blockType, BinaryReader fileReader)
        {
            if (!TypeList.ContainsKey(blockType))
                throw new Exception("TypeList does not contain blockType!");
            switch (TypeList[blockType])
            {
                case "str":
                    return fileReader.ReadUInt32();
                case "int":
                    return fileReader.ReadInt32();
                case "float":
                    return fileReader.ReadSingle();
                case "typex":
                    fileReader.BaseStream.Seek(2, SeekOrigin.Current);
                    return fileReader.ReadByte();
                case "bool":
                    fileReader.BaseStream.Seek(2, SeekOrigin.Current);
                    return fileReader.ReadByte();
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
        private static void FileTypeErr(BinaryReader fileInput)
        {
            // Todo: Add File Name to Exception.
            if (ByteArrayToString(fileInput.ReadBytes(4)) != "00424246")
                throw new Exception($"Error: File is not a proper .blk file.");
        }

        /// <summary>
        /// Converts bytes to hex string.
        /// </summary>
        /// <remarks>
        /// https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa
        /// </remarks>
        public static string ByteArrayToString(byte[] arrBytes)
        {
            var hex = BitConverter.ToString(arrBytes);
            return hex.Replace("-", "");
        }
    }
}