using System;
using System.Collections.Generic;

namespace OJT___QR_Code_Generator
{
    internal class WHA_Data
    {
        // Dictionary pairing the Bin Location (Key) with its associated Part Number (Value)
        public static readonly Dictionary<string, string> BinToPartMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // WHA01
            { "WHA01-B-1", "ACXB15-19870" },
            { "WHA01-C-1", "ACXB15-19850" },
            { "WHA01-C-2", "ACXB15-19880" },
            { "WHA01-C-3", "ACXB15-22880" },
            { "WHA01-D-1", "ACXB15-19830" },
            { "WHA01-D-2", "ACXB15-21750" },
            { "WHA01-D-3", "ACXB15-22840" },
            { "WHA01-D-4", "ACXB15-19800" },
            { "WHA01-E-1", "ACXB15-19820" },
            { "WHA01-E-2", "ACXB15-22820" },
            { "WHA01-E-3", "ACXB15-22830" },
            { "WHA01-E-4", "ACXB15-19810" },

            // WHA02
            { "WHA02-1A-1", "Z83G50" },
            { "WHA02-1A-2", "T8EX005" },
            { "WHA02-1B-1", "Z83G50" },
            { "WHA02-1C-1", "Z83G50" },
            { "WHA02-2A-1", "Z12NS2B24" },
            { "WHA02-2B-1", "Z12NS2B24" },
            { "WHA02-2C-1", "Z12BCUP2B24" },
            { "WHA02-3A-1", "Z33C12X55JL-F" },
            { "WHA02-3B-1", "Z33C25X55JL-F" },
            { "WHA02-3C-1", "Z35P15X50LD" },

            // WHA03
            { "WHA03-A-1", "Z8898L" },
            { "WHA03-A-2", "Z31F50X55AL" },
            { "WHA03-B-1", "Z8898L" },
            { "WHA03-B-2", "Z31F12X55JL" },
            { "WHA03-B-3", "Z31F50X55AL" },
            { "WHA03-C-1", "4601085" },
            { "WHA03-C-2", "4601089" },
            { "WHA03-C-3", "ZPP-19-180L-SU" },

            // WHA04
            { "WHA04-A-1", "K1YZ01000026" },
            { "WHA04-A-2", "K1YZ01000027" },
            { "WHA04-A-3", "K1YZ01000028" },
            { "WHA04-A-4", "K1YZ01000029" },
            { "WHA04-B-1", "Z904S15X34JL-A" },
            { "WHA04-B-2", "06-284240JL" },
            { "WHA04-B-3", "GL1350G" },
            { "WHA04-C-1", "Z62MF100BR" },
            { "WHA04-C-2", "IPM00002" },
            { "WHA04-C-3", "Z62MSF10B" },

            // WHA05
            { "WHA05-A-1", "ACXT07K10861" },
            { "WHA05-A-2", "ACXT00-75190" },
            { "WHA05-A-3", "ACXT00-76490" },
            { "WHA05-A-4", "ACXT00-87290" },
            { "WHA05-A-5", "ACXT00-87220" },
            { "WHA05-B-1", "ACXT07K08670" },
            { "WHA05-B-2", "ACXT07K08620" },
            { "WHA05-B-3", "ACXT07K08750" },
            { "WHA05-B-4", "ACXT30-96080" },
            { "WHA05-B-5", "ACXT30-96070" },
            { "WHA05-B-6", "ACXT30-96060" },
            { "WHA05-C-1", "ACXT30-99740" },
            { "WHA05-C-2", "ACXT30-99730" },
            { "WHA05-C-3", "ACXT30-98930" },
            { "WHA05-C-4", "ACXT30-95980" },
            { "WHA05-C-5", "ACXT00-87770" },

            // WHA06
            { "WHA06-A-1", "ACXT07K10812" },
            { "WHA06-A-2", "ACXT00-83690" },
            { "WHA06-B-1", "ACXT300418" },
            { "WHA06-B-2", "T023577" },
            { "WHA06-B-3", "ACXT30-97070" },
            { "WHA06-B-4", "ACXT30-97060" },
            { "WHA06-B-5", "ACXT30-95810" },
            { "WHA06-C-1", "ACXT31-17950" },
            { "WHA06-C-2", "ACXT00-76500" },
            { "WHA06-C-3", "ACXT00-84221" },

            // WHA07
            { "WHA07-A-1", "ACXT00-84890" },
            { "WHA07-A-2", "ACXT00-84830" },
            { "WHA07-A-3", "ACXT00-85220" },
            { "WHA07-A-4", "ACXT00-75560" },
            { "WHA07-A-5", "ACXT00-83630" },
            { "WHA07-B-1", "ACXT30-97560" },
            { "WHA07-B-2", "ACXT00-60610" },
            { "WHA07-B-3", "ACXT00-61120" },
            { "WHA07-B-4", "ACXT00-60620" },
            { "WHA07-C-1", "ACXT00K00810" },
            { "WHA07-C-2", "ACXT00-85210" },
            { "WHA07-C-4", "ACXT00K01720" },
            { "WHA07-C-5", "ACXT00K01730" },

            // WHA08
            { "WHA08-A-1", "ACXT00-84291" },
            { "WHA08-A-2", "ACXT00-84261" },
            { "WHA08-B-1", "ACXT00-57100" },
            { "WHA08-B-2", "ACXT07K0028" },
            { "WHA08-B-3", "ACXT07K0027" },
            { "WHA08-B-4", "ACXT00-64420" },
            { "WHA08-B-5", "ACXT00-60960" },
            { "WHA08-C-1", "ACXT00K02243" },
            { "WHA08-C-2", "ACXT00K02390" },
            { "WHA08-C-3", "ACXT00K00820" },
            { "WHA08-C-4", "ACXT30-95830" },
            { "WHA08-C-5", "ACXT00-61290" },
            { "WHA08-C-6", "ACXT00-66810" },
            { "WHA08-D1", "ACXA42-00410" },
            { "WHA08-D2", "ACXA42-00440" },
            { "WHA08-D3", "ACXA42-00490" },
            { "WHA08-D4", "G0C392J00065" },
            { "WHA08-D5", "G0C702K00002" },

            // WHA09
            { "WHA09-A-1", "ACXG30-12990" },
            { "WHA09-A-2", "7070231U" },
            { "WHA09-A-3", "7070242U" },

            // WHA10
            { "WHA10-A-1", "D001340" },
            { "WHA10-A-4", "7070222U" },
            { "WHA10-B-2", "H171039A" },
            { "WHA10-B-3", "H171012" },

            // WHA11
            { "WHA11-A-1", "ACXG30-11930" },
            { "WHA11-A-2", "G302160" },

            // WHA12
            { "WHA12-A-1", "ACXG30-13000" },
            { "WHA12-A-2", "ACXG30-12961" },
            { "WHA12-A-3", "ACXG30-12650" },

            // WHA13
            { "WHA13-1A-1", "7070668" },
            { "WHA13-1A-2", "7070632U" },
            { "WHA13-1B-1", "7070673U" },
            { "WHA13-1C-1", "7070317" },
            { "WHA13-2A-1", "7070616" },
            { "WHA13-2B-1", "7070621U" },
            { "WHA13-2B-2", "7070620U" },
            { "WHA13-2C-1", "7080310" },

            // WHA14
            { "WHA14-1A-1", "ACXG30-16390" },
            { "WHA14-1A-2", "ACXG30-16390" },
            { "WHA14-1B-1", "ACXG30-14241" },
            { "WHA14-1B-2", "ACXG30-11510" },
            { "WHA14-1B-3", "H171041" },
            { "WHA14-1C-1", "ACXG30-12000" },
            { "WHA14-1C-2", "ACXG30-11890" },
            { "WHA14-1C-3", "ACXG30-14430" },
            { "WHA14-2B-1", "ACXG30-12950" },
            { "WHA14-2B-2", "ACXG30-09812" },
            { "WHA14-2B-3", "ACXG30-16380" },
            { "WHA14-2C-1", "ACXG30-11172" },
            { "WHA14-2C-2", "ACXG30-11520" },
            { "WHA14-2C-3", "ACXG30-10760" },
            { "WHA14-3A-1", "ACXG30-11182" },
            { "WHA14-3A-2", "ACXG30-11500" },
            { "WHA14-3A-3", "ACXG30-12890" },
            { "WHA14-3B-1", "ACXG30-11960" },
            { "WHA14-3B-2", "G302161" },
            { "WHA14-3B-3", "ACXG30-11440" },
            { "WHA14-3C-1", "ACXG30-11900" },
            { "WHA14-3C-2", "ACXG30-12010" },
            { "WHA14-3C-3", "KG8AF90-90" },
            { "WHA14-4A-1", "ACXG30-12881" },
            { "WHA14-4A-2", "ACXG30-16430" },
            { "WHA14-4A-3", "ACXG30-16410" },
            { "WHA14-4B-1", "KG5AF120-140" },
            { "WHA14-4B-2", "KG5AF50-80" },
            { "WHA14-4B-3", "KG8AF25-40" },
            { "WHA14-4C-1", "KG8AF70-90" },
            { "WHA14-4C-2", "KG5AF50-150" },
            { "WHA14-4C-3", "KG3AF90-90" },
            { "WHA14-5A-1", "ACXG30-12171" },
            { "WHA14-5A-2", "ACXG30-12660" },
            { "WHA14-5B-1", "KG3AF50-50" },
            { "WHA14-5B-2", "KG3AF60-120" },
            { "WHA14-5B-3", "KG3AF50-160" },
            { "WHA14-5C-1", "KG3AF90-150" },
            { "WHA14-5C-2", "KG3AF50-250" },
            { "WHA14-5C-3", "KG3AF200-300" },
            { "WHA14-6A-1", "ACXG30-09822" },
            { "WHA14-6A-2", "ACXG30-12670" },
            { "WHA14-6A-3", "ACXG30-11260" },
            { "WHA14-6B-1", "KG3AF22-265" },
            { "WHA14-6B-2", "KG3AF40-120" },
            { "WHA14-6B-3", "KG3AF20-80" },
            { "WHA14-6B-4", "KG3AF20-130" },
            { "WHA14-6B-5", "KG3AF20-140" },
            { "WHA14-6C-1", "KG5AF60-200" },
            { "WHA14-6C-2", "KG3AF150-300" },
            { "WHA14-6C-3", "KG3AF150-280" },
            { "WHA14-7A-1", "ACXG30-12101" },
            { "WHA14-7A-2", "ACXG30-11920" },
            { "WHA14-7A-3", "ACXG30-11910" },
            { "WHA14-7B-1", "KG3AF100-180" },
            { "WHA14-7B-2", "KG3AF100-200" },
            { "WHA14-7B-3", "KG3AF120-180" },
            { "WHA14-7B-4", "KG3AF100-150" },
            { "WHA14-7C-1", "KG3AF150-170" },
            { "WHA14-7C-2", "KG3AF150-150" },
            { "WHA14-7C-3", "KG3AF100-160" },

            // WHA15
            { "WHA15-1A-1", "KG3AF100-100" },
            { "WHA15-1A-2", "KG2A80-100" },
            { "WHA15-1B-1", "KG2AF70-100" },
            { "WHA15-1B-2", "KG2AF50-50" },
            { "WHA15-1B-3", "KG2AF150-200" },
            { "WHA15-1C-1", "KG2AF50-160" },
            { "WHA15-1C-2", "KG2AF200-200" },
            { "WHA15-2A-1", "KG3AF40-160" },
            { "WHA15-2A-2", "5553010" },
            { "WHA15-2B-1", "H861190" },
            { "WHA15-2B-2", "TB2A50-50" },
            { "WHA15-2C-1", "KG2AF50-100" },
            { "WHA15-2C-2", "KG2AF30-40" },
            { "WHA15-2C-3", "KG2AF40-80" }
        };

        /// <summary>
        /// Safe retrieval helper to query a Part Number from a specific Bin Location.
        /// Returns an empty string if the bin location doesn't exist.
        /// </summary>
        public static string GetPartInfo(string binLocation)
        {
            if (string.IsNullOrWhiteSpace(binLocation)) return string.Empty;

            if (BinToPartMapping.TryGetValue(binLocation.Trim(), out string partNumber))
            {
                return partNumber;
            }
            return string.Empty;
        }
    }
}