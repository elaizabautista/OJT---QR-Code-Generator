using System;
using System.Collections.Generic;
using System.Text;

namespace OJT___QR_Code_Generator
{
    // The Data Model
    public class BinEntry
    {
        public string BinLocation { get; set; }
        public string Material { get; set; }
        public string MaterialDescription { get; set; }
    }

    // The Global State Manager
    public static class SharedWarehouseData
    {
        // This replaces the private _uploadedZones in Form1
        public static Dictionary<string, List<BinEntry>> UploadedZones { get; set; } = new Dictionary<string, List<BinEntry>>();
    }

    // Natural Sort Helper for Windows (Moved here so all forms can use it)
    public class NaturalStringComparer : IComparer<string>
    {
        [System.Runtime.InteropServices.DllImport("shlwapi.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string psz1, string psz2);

        public int Compare(string x, string y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return StrCmpLogicalW(x, y);
        }
    }
}