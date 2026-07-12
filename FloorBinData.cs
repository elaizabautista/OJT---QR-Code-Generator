using System.Collections.Generic;

namespace OJT___QR_Code_Generator
{
    public static class FloorBin_Data
    {
        // Maps each WHA warehouse zone to every floor bin location code within it.
        // Sourced directly from the FINAL_MARD_Material_Bin_AC Excel export.
        public static readonly Dictionary<string, string[]> ZoneToBins =
            new Dictionary<string, string[]>
        {

        {
            "WHA01", new string[]
            {
                "WHA01-B-1",
                "WHA01-C-1",
                "WHA01-C-2",
                "WHA01-C-3",
                "WHA01-D-1",
                "WHA01-D-2",
                "WHA01-D-3",
                "WHA01-D-4",
                "WHA01-E-1",
                "WHA01-E-2",
                "WHA01-E-3",
                "WHA01-E-4"
            }
        },

        {
            "WHA02", new string[]
            {
                "WHA02-1A-1",
                "WHA02-1A-2",
                "WHA02-1B-1",
                "WHA02-1C-1",
                "WHA02-2A-1",
                "WHA02-2B-1",
                "WHA02-2C-1",
                "WHA02-3A-1",
                "WHA02-3B-1",
                "WHA02-3C-1"
            }
        },

        {
            "WHA03", new string[]
            {
                "WHA03-A-1",
                "WHA03-A-2",
                "WHA03-B-1",
                "WHA03-B-2",
                "WHA03-B-3",
                "WHA03-C-1",
                "WHA03-C-2",
                "WHA03-C-3"
            }
        },

        {
            "WHA04", new string[]
            {
                "WHA04-A-1",
                "WHA04-A-2",
                "WHA04-A-3",
                "WHA04-A-4",
                "WHA04-B-1",
                "WHA04-B-2",
                "WHA04-B-3",
                "WHA04-C-1",
                "WHA04-C-2",
                "WHA04-C-3"
            }
        },

        {
            "WHA05", new string[]
            {
                "WHA05-A-1",
                "WHA05-A-2",
                "WHA05-A-3",
                "WHA05-A-4",
                "WHA05-A-5",
                "WHA05-B-1",
                "WHA05-B-2",
                "WHA05-B-3",
                "WHA05-B-4",
                "WHA05-B-5",
                "WHA05-B-6",
                "WHA05-C-1",
                "WHA05-C-2",
                "WHA05-C-3",
                "WHA05-C-4",
                "WHA05-C-5"
            }
        },

        {
            "WHA06", new string[]
            {
                "WHA06-A-1",
                "WHA06-A-2",
                "WHA06-B-1",
                "WHA06-B-2",
                "WHA06-B-3",
                "WHA06-B-4",
                "WHA06-B-5",
                "WHA06-C-1",
                "WHA06-C-2",
                "WHA06-C-3"
            }
        },

        {
            "WHA07", new string[]
            {
                "WHA07-A-1",
                "WHA07-A-2",
                "WHA07-A-3",
                "WHA07-A-4",
                "WHA07-A-5",
                "WHA07-B-1",
                "WHA07-B-2",
                "WHA07-B-3",
                "WHA07-B-4",
                "WHA07-C-1",
                "WHA07-C-2",
                "WHA07-C-4",
                "WHA07-C-5"
            }
        },

        {
            "WHA08", new string[]
            {
                "WHA08-A-1",
                "WHA08-A-2",
                "WHA08-B-1",
                "WHA08-B-2",
                "WHA08-B-3",
                "WHA08-B-4",
                "WHA08-B-5",
                "WHA08-C-1",
                "WHA08-C-2",
                "WHA08-C-3",
                "WHA08-C-4",
                "WHA08-C-5",
                "WHA08-C-6",
                "WHA08-D1",
                "WHA08-D2",
                "WHA08-D3",
                "WHA08-D4",
                "WHA08-D5"
            }
        },

        {
            "WHA09", new string[]
            {
                "WHA09-A-1",
                "WHA09-A-2",
                "WHA09-A-3"
            }
        },

        {
            "WHA10", new string[]
            {
                "WHA10-A-1",
                "WHA10-A-4",
                "WHA10-B-2",
                "WHA10-B-3"
            }
        },

        {
            "WHA11", new string[]
            {
                "WHA11-A-1",
                "WHA11-A-2"
            }
        },

        {
            "WHA12", new string[]
            {
                "WHA12-A-1",
                "WHA12-A-2",
                "WHA12-A-3"
            }
        },

        {
            "WHA13", new string[]
            {
                "WHA13-1A-1",
                "WHA13-1A-2",
                "WHA13-1B-1",
                "WHA13-1C-1",
                "WHA13-2A-1",
                "WHA13-2B-1",
                "WHA13-2B-2",
                "WHA13-2C-1"
            }
        },

        {
            "WHA14", new string[]
            {
                "WHA14-1A-1",
                "WHA14-1A-2",
                "WHA14-1B-1",
                "WHA14-1B-2",
                "WHA14-1B-3",
                "WHA14-1C-1",
                "WHA14-1C-2",
                "WHA14-1C-3",
                "WHA14-2B-1",
                "WHA14-2B-2",
                "WHA14-2B-3",
                "WHA14-2C-1",
                "WHA14-2C-2",
                "WHA14-2C-3",
                "WHA14-3A-1",
                "WHA14-3A-2",
                "WHA14-3A-3",
                "WHA14-3B-1",
                "WHA14-3B-2",
                "WHA14-3B-3",
                "WHA14-3C-1",
                "WHA14-3C-2",
                "WHA14-3C-3",
                "WHA14-4A-1",
                "WHA14-4A-2",
                "WHA14-4A-3",
                "WHA14-4B-1",
                "WHA14-4B-2",
                "WHA14-4B-3",
                "WHA14-4C-1",
                "WHA14-4C-2",
                "WHA14-4C-3",
                "WHA14-5A-1",
                "WHA14-5A-2",
                "WHA14-5B-1",
                "WHA14-5B-2",
                "WHA14-5B-3",
                "WHA14-5C-1",
                "WHA14-5C-2",
                "WHA14-5C-3",
                "WHA14-6A-1",
                "WHA14-6A-2",
                "WHA14-6A-3",
                "WHA14-6B-1",
                "WHA14-6B-2",
                "WHA14-6B-3",
                "WHA14-6B-4",
                "WHA14-6B-5",
                "WHA14-6C-1",
                "WHA14-6C-2",
                "WHA14-6C-3",
                "WHA14-7A-1",
                "WHA14-7A-2",
                "WHA14-7A-3",
                "WHA14-7B-1",
                "WHA14-7B-2",
                "WHA14-7B-3",
                "WHA14-7B-4",
                "WHA14-7C-1",
                "WHA14-7C-2",
                "WHA14-7C-3"
            }
        },

        {
            "WHA15", new string[]
            {
                "WHA15-1A-1",
                "WHA15-1A-2",
                "WHA15-1B-1",
                "WHA15-1B-2",
                "WHA15-1B-3",
                "WHA15-1C-1",
                "WHA15-1C-2",
                "WHA15-2A-1",
                "WHA15-2A-2",
                "WHA15-2B-1",
                "WHA15-2B-2",
                "WHA15-2C-1",
                "WHA15-2C-2",
                "WHA15-2C-3"
            }
        },

        {
            "WHA-CP", new string[]
            {
                "WHA-CP01-1",
                "WHA-CP01-2",
                "WHA-CP02-1",
                "WHA-CP02-2",
                "WHA-CP03-1",
                "WHA-CP03-2",
                "WHA-CP04-1",
                "WHA-CP04-2",
                "WHA-CP05-1",
                "WHA-CP05-2",
                "WHA-CP06-1",
                "WHA-CP06-2",
                "WHA-CP07-1",
                "WHA-CP07-2",
                "WHA-CP08-1",
                "WHA-CP08-2",
                "WHA-CP09-1",
                "WHA-CP10-1",
                "WHA-CP11-1",
                "WHA-CP11-2",
                "WHA-CP12-1"
            }
        },

        {
            "WHA-FM", new string[]
            {
                "WHA-FM-1-1",
                "WHA-FM-2-1",
                "WHA-FM-3-1",
                "WHA-FM-4-1",
                "WHA-FM-5-1",
                "WHA-FM-6-1",
                "WHA-FM-7-1",
                "WHA-FM-8-1",
                "WHA-FM-9-1",
                "WHA-FM-10-1",
                "WHA-FM-11-1",
                "WHA-FM-12-1",
                "WHA-FM-13-1",
                "WHA-FM-14-1",
                "WHA-FM-15-1",
                "WHA-FM-16-1",
                "WHA-FM-17-1",
                "WHA-FM-18-1",
                "WHA-FM19-1",
                "WHA-FM20-1"
            }
        },

        {
            "WHC", new string[]
            {
                "WHC-01-1",
                "WHC-01-2",
                "WHC-01-3",
                "WHC-01-4",
                "WHC-01-5",
                "WHC-01-6",
                "WHC-01-7",
                "WHC-01-8",
                "WHC-01-9",
                "WHC-02-1",
                "WHC-02-2",
                "WHC-03-1",
                "WHC-03-2",
                "WHC-04-1",
                "WHC-04-2",
                "WHC-04-3",
                "WHC-05-1",
                "WHC-05-2",
                "WHC-06-1",
                "WHC-06-2",
                "WHC-07-1",
                "WHC-07-2",
                "WHC-07-3",
                "WHC-07-4",
                "WHC-08-1",
                "WHC-09-1",
                "WHC-09-2",
                "WHC-10-1",
                "WHC-10-2",
                "WHC-10-3",
                "WHC-11-1",
                "WHC-12-1",
                "WHC-12-2",
                "WHC-12-3",
                "WHC-13-1",
                "WHC-13-2",
                "WHC-13-3",
                "WHC-14-1",
                "WHC-14-2",
                "WHC-14-3",
                "WHC-15-1",
                "WHC-15-2",
                "WHC-16-1",
                "WHC-17-1",
                "WHC-17-2",
                "WHC-18-1",
                "WHC-18-2",
                "WHC-19-1",
                "WHC-19-2",
                "WHC-20-1",
                "WHC-20-2",
                "WHC-21-1",
                "WHC-21-2",
                "WHC-22-1",
                "WHC-22-2",
                "WHC-23-1",
                "WHC-23-2",
                "WHC-24-1",
                "WHC-24-2",
                "WHC-24-3",
                "WHC-24-4"
            }
        },
        };
    }
}
