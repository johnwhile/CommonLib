using System;
using System.Collections.Generic;
using System.Text;

using Common.Maths;
using Common.Tools;

namespace Common.Partitions
{
    [Flags]
    public enum VoxelCase : byte
    {
        EMPTY,
        P0 = 1,
        P1 = 2,
        P2 = 4,
        P3 = 8,
        P4 = 16,
        P5 = 32,
        P6 = 64,
        P7 = 128,
        FULL = 255
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// bits 1,2 = xsign
    /// bits 3,4 = ysign
    /// bits 5,6 = zsign
    /// </remarks>
    [Flags]
    public enum VoxelAxe : byte
    {
        Xpositive = 1,
        Xnegative = 2,
        Ypositive = 4,
        Ynegative = 8,
        Zpositive = 16,
        Znegative = 32,
    }


    /// <summary>
    /// Precomputed constants data
    /// </summary>
    public static class VoxelConst3d
    {
        static byte[][] TriangleCase;

        // A WEEK TO DO ALL 255 CASES
        static VoxelConst3d()
        {
            // exist maximum 5 triangles for a case
            // and exist maximum 12 vertices for a case
            TriangleCase = new byte[256][];

            TriangleCase[0] = new byte[] { };
            TriangleCase[1] = new byte[] { 0, 8, 3 };
            TriangleCase[2] = new byte[] { 1, 9, 0 };
            TriangleCase[3] = new byte[] { 1, 9, 8, 1, 8, 3 };
            TriangleCase[4] = new byte[] { 2, 10, 1 };
            TriangleCase[5] = new byte[] { 0, 8, 3, 2, 10, 1 };
            TriangleCase[6] = new byte[] { 2, 9, 0, 2, 10, 9 };
            TriangleCase[7] = new byte[] { 2, 8, 3, 2, 10, 8, 10, 9, 8 };
            TriangleCase[8] = new byte[] { 3, 11, 2 };
            TriangleCase[9] = new byte[] { 2, 0, 8, 2, 8, 11 };
            TriangleCase[10] = new byte[] { 1, 9, 0, 3, 11, 2 };
            TriangleCase[11] = new byte[] { 8, 11, 9, 9, 11, 2, 9, 2, 1 };
            TriangleCase[12] = new byte[] { 3, 11, 10, 3, 10, 1 };
            TriangleCase[13] = new byte[] { 1, 0, 8, 10, 1, 8, 11, 10, 8 };
            TriangleCase[14] = new byte[] { 9, 0, 3, 9, 3, 11, 9, 11, 10 };
            TriangleCase[15] = new byte[] { 8, 10, 9, 8, 11, 10 };
            TriangleCase[16] = new byte[] { 4, 7, 8 };
            TriangleCase[17] = new byte[] { 3, 0, 4, 3, 4, 7 };
            TriangleCase[18] = new byte[] { 1, 9, 0, 4, 7, 8 };
            TriangleCase[19] = new byte[] { 7, 3, 1, 7, 1, 9, 7, 9, 4 };
            TriangleCase[20] = new byte[] { 2, 10, 1, 4, 7, 8 };
            TriangleCase[21] = new byte[] { 2, 10, 1, 3, 0, 4, 3, 4, 7 };
            TriangleCase[22] = new byte[] { 2, 9, 0, 2, 10, 9, 4, 7, 8 };
            TriangleCase[23] = new byte[] { 2, 7, 3, 2, 10, 7, 7, 10, 4, 4, 10, 9 };
            TriangleCase[24] = new byte[] { 3, 11, 2, 4, 7, 8 };
            TriangleCase[25] = new byte[] { 2, 0, 4, 2, 4, 7, 2, 7, 11 };
            TriangleCase[26] = new byte[] { 1, 9, 0, 3, 11, 2, 4, 7, 8 };
            TriangleCase[27] = new byte[] { 1, 11, 2, 1, 9, 11, 9, 4, 11, 11, 4, 7 };
            TriangleCase[28] = new byte[] { 3, 11, 1, 1, 11, 10, 4, 7, 8 };
            TriangleCase[29] = new byte[] { 0, 4, 1, 1, 4, 10, 10, 4, 7, 10, 7, 11 };
            TriangleCase[30] = new byte[] { 4, 7, 8, 9, 0, 3, 9, 3, 11, 9, 11, 10 };
            TriangleCase[31] = new byte[] { 10, 9, 4, 10, 4, 7, 10, 7, 11 };
            TriangleCase[32] = new byte[] { 5, 4, 9 };
            TriangleCase[33] = new byte[] { 0, 8, 3, 5, 4, 9 };
            TriangleCase[34] = new byte[] { 1, 5, 4, 1, 4, 0 };
            TriangleCase[35] = new byte[] { 3, 1, 5, 3, 5, 8, 8, 5, 4 };
            TriangleCase[36] = new byte[] { 2, 10, 1, 5, 4, 9 };
            TriangleCase[37] = new byte[] { 0, 8, 3, 2, 10, 1, 5, 4, 9 };
            TriangleCase[38] = new byte[] { 4, 0, 2, 4, 2, 10, 4, 10, 5 };
            TriangleCase[39] = new byte[] { 2, 8, 3, 2, 10, 8, 8, 10, 5, 8, 5, 4 };
            TriangleCase[40] = new byte[] { 3, 11, 2, 5, 4, 9 };
            TriangleCase[41] = new byte[] { 8, 2, 0, 8, 11, 2, 5, 4, 9 };
            TriangleCase[42] = new byte[] { 0, 1, 5, 0, 5, 4, 3, 11, 2 };
            TriangleCase[43] = new byte[] { 2, 1, 5, 8, 11, 4, 4, 11, 5, 5, 11, 2 };
            TriangleCase[44] = new byte[] { 1, 3, 11, 1, 11, 10, 5, 4, 9 };
            TriangleCase[45] = new byte[] { 8, 11, 10, 8, 10, 1, 8, 1, 0, 5, 4, 9 };
            TriangleCase[46] = new byte[] { 4, 0, 3, 4, 3, 11, 10, 5, 11, 11, 5, 4 };
            TriangleCase[47] = new byte[] { 4, 8, 11, 10, 5, 11, 11, 5, 4 };
            TriangleCase[48] = new byte[] { 8, 9, 5, 8, 5, 7 };
            TriangleCase[49] = new byte[] { 7, 3, 5, 5, 3, 0, 5, 0, 9 };
            TriangleCase[50] = new byte[] { 1, 5, 7, 1, 7, 8, 1, 8, 0 };
            TriangleCase[51] = new byte[] { 1, 5, 7, 1, 7, 3 };
            TriangleCase[52] = new byte[] { 2, 10, 1, 8, 9, 5, 8, 5, 7 };
            TriangleCase[53] = new byte[] { 2, 10, 1, 7, 3, 5, 5, 3, 0, 5, 0, 9 };
            TriangleCase[54] = new byte[] { 2, 10, 7, 7, 10, 5, 2, 7, 8, 2, 8, 0 };
            TriangleCase[55] = new byte[] { 7, 10, 5, 7, 2, 10, 7, 3, 2 };
            TriangleCase[56] = new byte[] { 3, 11, 2, 8, 9, 5, 8, 5, 7 };
            TriangleCase[57] = new byte[] { 2, 0, 9, 2, 9, 5, 5, 7, 11, 5, 11, 2 };
            TriangleCase[58] = new byte[] { 3, 11, 2, 1, 5, 7, 1, 7, 8, 1, 8, 0 };
            TriangleCase[59] = new byte[] { 1, 5, 2, 2, 5, 11, 11, 5, 7 };
            TriangleCase[60] = new byte[] { 8, 9, 5, 8, 5, 7, 3, 11, 10, 3, 10, 1 };
            TriangleCase[61] = new byte[] { 0, 11, 1, 1, 11, 10, 0, 7, 11, 0, 9, 7, 9, 5, 7 };
            TriangleCase[62] = new byte[] { 11, 10, 3, 3, 10, 0, 0, 10, 5, 0, 5, 8, 8, 5, 7 };
            TriangleCase[63] = new byte[] { 7, 11, 10, 7, 10, 5 };
            TriangleCase[64] = new byte[] { 6, 5, 10 };
            TriangleCase[65] = new byte[] { 0, 8, 3, 6, 5, 10 };
            TriangleCase[66] = new byte[] { 1, 9, 0, 6, 5, 10 };
            TriangleCase[67] = new byte[] { 1, 9, 8, 1, 8, 3, 6, 5, 10 };
            TriangleCase[68] = new byte[] { 2, 6, 5, 2, 5, 1 };
            TriangleCase[69] = new byte[] { 0, 8, 3, 2, 6, 5, 2, 5, 1 };
            TriangleCase[70] = new byte[] { 9, 6, 5, 9, 0, 6, 6, 0, 2 };
            TriangleCase[71] = new byte[] { 2, 6, 3, 3, 6, 8, 8, 6, 5, 8, 5, 9 };
            TriangleCase[72] = new byte[] { 3, 11, 2, 6, 5, 10 };
            TriangleCase[73] = new byte[] { 2, 0, 8, 2, 8, 11, 6, 5, 10 };
            TriangleCase[74] = new byte[] { 1, 9, 0, 3, 11, 2, 6, 5, 10 };
            TriangleCase[75] = new byte[] { 9, 8, 11, 9, 11, 1, 1, 11, 2, 6, 5, 10 };
            TriangleCase[76] = new byte[] { 5, 1, 3, 5, 3, 11, 5, 11, 6 };
            TriangleCase[77] = new byte[] { 1, 0, 8, 1, 8, 11, 1, 11, 6, 1, 6, 5 };
            TriangleCase[78] = new byte[] { 0, 3, 9, 9, 3, 11, 11, 5, 9, 11, 6, 5 };
            TriangleCase[79] = new byte[] { 5, 9, 8, 11, 6, 8, 8, 6, 5 };
            TriangleCase[80] = new byte[] { 4, 7, 8, 6, 5, 10 };
            TriangleCase[81] = new byte[] { 0, 4, 3, 3, 4, 7, 6, 5, 10 };
            TriangleCase[82] = new byte[] { 0, 1, 9, 4, 7, 8, 6, 5, 10 };
            TriangleCase[83] = new byte[] { 4, 7, 1, 7, 3, 1, 4, 1, 9, 6, 5, 10 };
            TriangleCase[84] = new byte[] { 4, 7, 8, 2, 6, 5, 2, 5, 1 };
            TriangleCase[85] = new byte[] { 2, 6, 5, 2, 5, 1, 3, 0, 4, 3, 4, 7 };
            TriangleCase[86] = new byte[] { 4, 7, 8, 0, 2, 6, 0, 6, 9, 9, 6, 5 };
            TriangleCase[87] = new byte[] { 7, 3, 4, 4, 3, 9, 9, 3, 2, 9, 2, 6, 9, 6, 5 };
            TriangleCase[88] = new byte[] { 3, 11, 2, 4, 7, 8, 6, 5, 10 };
            TriangleCase[89] = new byte[] { 2, 0, 4, 2, 4, 7, 2, 7, 11, 6, 5, 10 };
            TriangleCase[90] = new byte[] { 1, 9, 0, 3, 11, 2, 4, 7, 8, 6, 5, 10 };
            TriangleCase[91] = new byte[] { 1, 9, 4, 1, 4, 7, 1, 7, 2, 2, 7, 11, 6, 5, 10 };
            TriangleCase[92] = new byte[] { 5, 1, 3, 5, 3, 6, 6, 3, 11, 4, 7, 8 };
            TriangleCase[93] = new byte[] { 11, 1, 0, 11, 0, 4, 11, 4, 7, 1, 6, 5, 1, 11, 6 };
            TriangleCase[94] = new byte[] { 4, 7, 8, 0, 3, 9, 9, 3, 5, 5, 3, 11, 5, 11, 6 };
            TriangleCase[95] = new byte[] { 4, 7, 11, 4, 11, 9, 9, 11, 5, 5, 11, 6 };
            TriangleCase[96] = new byte[] { 4, 9, 10, 4, 10, 6 };
            TriangleCase[97] = new byte[] { 0, 8, 3, 4, 9, 10, 4, 10, 6 };
            TriangleCase[98] = new byte[] { 6, 4, 0, 6, 0, 10, 10, 0, 1 };
            TriangleCase[99] = new byte[] { 3, 1, 8, 4, 8, 1, 4, 1, 10, 4, 10, 6 };
            TriangleCase[100] = new byte[] { 4, 2, 6, 4, 1, 2, 4, 9, 1 };
            TriangleCase[101] = new byte[] { 0, 8, 3, 4, 2, 6, 4, 1, 2, 4, 9, 1 };
            TriangleCase[102] = new byte[] { 0, 2, 4, 2, 6, 4 };
            TriangleCase[103] = new byte[] { 4, 8, 3, 4, 3, 2, 4, 2, 6 };
            TriangleCase[104] = new byte[] { 3, 11, 2, 4, 9, 10, 4, 10, 6 };
            TriangleCase[105] = new byte[] { 4, 9, 10, 4, 10, 6, 2, 0, 8, 2, 8, 11 };
            TriangleCase[106] = new byte[] { 6, 4, 0, 6, 0, 10, 10, 0, 1, 3, 11, 2 };
            TriangleCase[107] = new byte[] { 11, 2, 8, 8, 2, 1, 8, 1, 4, 4, 1, 10, 4, 10, 6 };
            TriangleCase[108] = new byte[] { 9, 1, 3, 4, 9, 3, 4, 3, 11, 4, 11, 6 };
            TriangleCase[109] = new byte[] { 6, 4, 9, 6, 9, 1, 6, 1, 11, 11, 1, 8, 8, 1, 0 };
            TriangleCase[110] = new byte[] { 4, 11, 6, 4, 3, 11, 4, 0, 3 };
            TriangleCase[111] = new byte[] { 4, 8, 11, 4, 11, 6 };
            TriangleCase[112] = new byte[] { 6, 7, 10, 10, 7, 8, 10, 8, 9 };
            TriangleCase[113] = new byte[] { 7, 3, 0, 7, 0, 9, 7, 9, 10, 7, 10, 6 };
            TriangleCase[114] = new byte[] { 0, 1, 8, 8, 1, 7, 7, 1, 10, 7, 10, 6 };
            TriangleCase[115] = new byte[] { 3, 1, 7, 7, 1, 10, 7, 10, 6 };
            TriangleCase[116] = new byte[] { 8, 9, 1, 8, 1, 2, 8, 2, 7, 7, 2, 6 };
            TriangleCase[117] = new byte[] { 7, 3, 0, 7, 0, 9, 7, 9, 6, 6, 9, 1, 6, 1, 2 };
            TriangleCase[118] = new byte[] { 8, 0, 2, 8, 2, 7, 7, 2, 6 };
            TriangleCase[119] = new byte[] { 3, 2, 7, 7, 2, 6 };
            TriangleCase[120] = new byte[] { 3, 11, 2, 8, 9, 10, 8, 10, 7, 7, 10, 6 };
            TriangleCase[121] = new byte[] { 7, 10, 6, 7, 9, 10, 7, 0, 9, 7, 2, 0, 7, 11, 2 };
            TriangleCase[122] = new byte[] { 7, 8, 0, 7, 0, 6, 6, 0, 1, 6, 1, 10, 2, 3, 11 };
            TriangleCase[123] = new byte[] { 7, 11, 2, 7, 2, 1, 1, 6, 7, 1, 10, 6 };
            TriangleCase[124] = new byte[] { 1, 3, 11, 1, 11, 6, 1, 6, 9, 9, 6, 7, 9, 7, 8 };
            TriangleCase[125] = new byte[] { 7, 11, 6, 0, 9, 1 };
            TriangleCase[126] = new byte[] { 0, 3, 11, 0, 11, 6, 0, 6, 7, 0, 7, 8 };
            TriangleCase[127] = new byte[] { 7, 11, 6 };
            TriangleCase[128] = new byte[] { 7, 6, 11 };
            TriangleCase[129] = new byte[] { 0, 8, 3, 7, 6, 11 };
            TriangleCase[130] = new byte[] { 1, 9, 0, 7, 6, 11 };
            TriangleCase[131] = new byte[] { 1, 9, 8, 1, 8, 3, 7, 6, 11 };
            TriangleCase[132] = new byte[] { 2, 10, 1, 7, 6, 11 };
            TriangleCase[133] = new byte[] { 0, 8, 3, 2, 10, 1, 7, 6, 11 };
            TriangleCase[134] = new byte[] { 2, 9, 0, 2, 10, 9, 7, 6, 11 };
            TriangleCase[135] = new byte[] { 2, 8, 3, 2, 10, 8, 10, 9, 8, 7, 6, 11 };
            TriangleCase[136] = new byte[] { 3, 7, 2, 7, 6, 2 };
            TriangleCase[137] = new byte[] { 7, 0, 8, 7, 6, 0, 6, 2, 0 };
            TriangleCase[138] = new byte[] { 1, 9, 0, 3, 7, 2, 7, 6, 2 };
            TriangleCase[139] = new byte[] { 8, 1, 9, 8, 2, 1, 8, 7, 2, 7, 6, 2 };
            TriangleCase[140] = new byte[] { 3, 7, 1, 7, 10, 1, 7, 6, 10 };
            TriangleCase[141] = new byte[] { 0, 8, 1, 8, 7, 1, 1, 7, 10, 7, 6, 10 };
            TriangleCase[142] = new byte[] { 7, 0, 3, 7, 9, 0, 7, 6, 9, 6, 10, 9 };
            TriangleCase[143] = new byte[] { 8, 10, 9, 8, 7, 10, 7, 6, 10 };
            TriangleCase[144] = new byte[] { 4, 11, 8, 4, 6, 11 };
            TriangleCase[145] = new byte[] { 3, 6, 11, 3, 0, 6, 0, 4, 6 };
            TriangleCase[146] = new byte[] { 1, 9, 0, 4, 11, 8, 4, 6, 11 };
            TriangleCase[147] = new byte[] { 4, 6, 11, 4, 11, 3, 4, 3, 1, 4, 1, 9 };
            TriangleCase[148] = new byte[] { 2, 10, 1, 4, 11, 8, 4, 6, 11 };
            TriangleCase[149] = new byte[] { 3, 6, 11, 3, 0, 6, 0, 4, 6, 2, 10, 1 };
            TriangleCase[150] = new byte[] { 2, 9, 0, 2, 10, 9, 4, 11, 8, 4, 6, 11 };
            TriangleCase[151] = new byte[] { 4, 6, 11, 4, 11, 3, 4, 3, 9, 9, 3, 2, 9, 2, 10 };
            TriangleCase[152] = new byte[] { 3, 8, 2, 2, 8, 4, 2, 4, 6 };
            TriangleCase[153] = new byte[] { 2, 0, 4, 2, 4, 6 };
            TriangleCase[154] = new byte[] { 1, 9, 0, 3, 8, 2, 2, 8, 4, 2, 4, 6 };
            TriangleCase[155] = new byte[] { 4, 1, 9, 4, 2, 1, 4, 6, 2 };
            TriangleCase[156] = new byte[] { 4, 6, 10, 4, 10, 1, 4, 1, 8, 8, 1, 3 };
            TriangleCase[157] = new byte[] { 4, 1, 0, 4, 10, 1, 4, 6, 10 };
            TriangleCase[158] = new byte[] { 10, 9, 0, 10, 0, 3, 10, 3, 6, 6, 3, 8, 6, 8, 4 };
            TriangleCase[159] = new byte[] { 4, 6, 10, 4, 10, 9 };
            TriangleCase[160] = new byte[] { 5, 4, 9, 7, 6, 11 };
            TriangleCase[161] = new byte[] { 0, 8, 3, 5, 4, 9, 7, 6, 11 };
            TriangleCase[162] = new byte[] { 1, 5, 4, 1, 4, 0, 7, 6, 11 };
            TriangleCase[163] = new byte[] { 1, 5, 3, 3, 5, 4, 3, 4, 8, 7, 6, 11 };
            TriangleCase[164] = new byte[] { 2, 10, 1, 5, 4, 9, 7, 6, 11 };
            TriangleCase[165] = new byte[] { 0, 8, 3, 2, 10, 1, 5, 4, 9, 7, 6, 11 };
            TriangleCase[166] = new byte[] { 4, 0, 2, 4, 2, 10, 4, 10, 5, 7, 6, 11 };
            TriangleCase[167] = new byte[] { 2, 8, 3, 2, 4, 8, 2, 10, 4, 4, 10, 5, 7, 6, 11 };
            TriangleCase[168] = new byte[] { 5, 4, 9, 3, 7, 2, 7, 6, 2 };
            TriangleCase[169] = new byte[] { 7, 0, 8, 7, 6, 0, 6, 2, 0, 4, 9, 5 };
            TriangleCase[170] = new byte[] { 1, 5, 4, 1, 4, 0, 3, 7, 2, 7, 6, 2 };
            TriangleCase[171] = new byte[] { 7, 6, 2, 7, 2, 8, 8, 2, 1, 8, 1, 4, 4, 1, 5 };
            TriangleCase[172] = new byte[] { 5, 4, 9, 3, 7, 1, 7, 10, 1, 7, 6, 10 };
            TriangleCase[173] = new byte[] { 5, 4, 9, 0, 8, 1, 8, 7, 1, 1, 7, 10, 7, 6, 10 };
            TriangleCase[174] = new byte[] { 3, 7, 6, 3, 6, 10, 3, 10, 0, 0, 10, 5, 0, 5, 4 };
            TriangleCase[175] = new byte[] { 8, 7, 6, 8, 6, 10, 10, 5, 4, 10, 4, 8 };
            TriangleCase[176] = new byte[] { 6, 9, 5, 6, 11, 9, 11, 8, 9 };
            TriangleCase[177] = new byte[] { 3, 0, 9, 3, 9, 5, 3, 5, 11, 5, 6, 11 };
            TriangleCase[178] = new byte[] { 1, 8, 0, 1, 5, 8, 8, 5, 6, 8, 6, 11 };
            TriangleCase[179] = new byte[] { 3, 1, 11, 11, 1, 5, 5, 6, 11 };
            TriangleCase[180] = new byte[] { 2, 10, 1, 5, 8, 9, 5, 11, 8, 5, 6, 11 };
            TriangleCase[181] = new byte[] { 3, 6, 11, 3, 0, 6, 0, 9, 6, 6, 9, 5, 2, 10, 1 };
            TriangleCase[182] = new byte[] { 0, 2, 10, 0, 10, 5, 0, 5, 8, 8, 5, 6, 8, 6, 11 };
            TriangleCase[183] = new byte[] { 3, 2, 10, 3, 10, 5, 5, 6, 11, 5, 11, 3 };
            TriangleCase[184] = new byte[] { 8, 9, 5, 8, 5, 6, 8, 6, 3, 3, 6, 2 };
            TriangleCase[185] = new byte[] { 2, 0, 9, 2, 9, 5, 2, 5, 6 };
            TriangleCase[186] = new byte[] { 1, 5, 0, 0, 5, 8, 8, 5, 6, 8, 6, 3, 6, 2, 3 };
            TriangleCase[187] = new byte[] { 2, 1, 5, 2, 5, 6 };
            TriangleCase[188] = new byte[] { 8, 9, 5, 8, 5, 6, 8, 6, 3, 3, 6, 10, 3, 10, 1 };
            TriangleCase[189] = new byte[] { 6, 10, 1, 6, 1, 0, 0, 9, 5, 0, 5, 6 };
            TriangleCase[190] = new byte[] { 3, 8, 0, 10, 5, 6 };
            TriangleCase[191] = new byte[] { 10, 5, 6 };
            TriangleCase[192] = new byte[] { 7, 5, 10, 7, 10, 11 };
            TriangleCase[193] = new byte[] { 7, 5, 10, 7, 10, 11, 0, 8, 3 };
            TriangleCase[194] = new byte[] { 7, 5, 10, 7, 10, 11, 1, 9, 0 };
            TriangleCase[195] = new byte[] { 7, 5, 10, 7, 10, 11, 1, 9, 8, 1, 8, 3 };
            TriangleCase[196] = new byte[] { 7, 5, 1, 7, 1, 2, 7, 2, 11 };
            TriangleCase[197] = new byte[] { 11, 1, 2, 11, 7, 1, 7, 5, 1, 3, 0, 8 };
            TriangleCase[198] = new byte[] { 0, 2, 9, 2, 5, 9, 2, 7, 5, 2, 11, 7 };
            TriangleCase[199] = new byte[] { 2, 8, 3, 2, 9, 8, 2, 5, 9, 2, 11, 5, 7, 5, 11 };
            TriangleCase[200] = new byte[] { 2, 5, 10, 2, 3, 5, 3, 7, 5 };
            TriangleCase[201] = new byte[] { 2, 0, 8, 2, 8, 7, 2, 7, 10, 5, 10, 7 };
            TriangleCase[202] = new byte[] { 2, 5, 10, 2, 3, 5, 3, 7, 5, 1, 9, 0 };
            TriangleCase[203] = new byte[] { 9, 8, 1, 1, 8, 2, 2, 8, 7, 7, 10, 2, 10, 7, 5 };
            TriangleCase[204] = new byte[] { 3, 7, 1, 7, 5, 1 };
            TriangleCase[205] = new byte[] { 0, 8, 1, 1, 8, 7, 1, 7, 5 };
            TriangleCase[206] = new byte[] { 0, 3, 7, 0, 7, 9, 9, 7, 5 };
            TriangleCase[207] = new byte[] { 8, 7, 9, 7, 5, 9 };
            TriangleCase[208] = new byte[] { 5, 8, 4, 5, 10, 8, 10, 11, 8 };
            TriangleCase[209] = new byte[] { 4, 5, 10, 4, 10, 11, 4, 11, 3, 4, 3, 0 };
            TriangleCase[210] = new byte[] { 1, 9, 0, 4, 5, 10, 4, 10, 11, 4, 11, 8 };
            TriangleCase[211] = new byte[] { 4, 5, 10, 4, 10, 11, 4, 11, 3, 4, 3, 1, 4, 1, 9 };
            TriangleCase[212] = new byte[] { 8, 2, 11, 4, 1, 8, 8, 1, 2, 1, 4, 5 };
            TriangleCase[213] = new byte[] { 5, 1, 2, 5, 2, 11, 5, 11, 4, 4, 11, 3, 4, 3, 0 };
            TriangleCase[214] = new byte[] { 11, 8, 4, 11, 4, 5, 11, 5, 2, 2, 5, 9, 2, 9, 0 };
            TriangleCase[215] = new byte[] { 3, 2, 11, 4, 5, 9 };
            TriangleCase[216] = new byte[] { 8, 4, 3, 4, 2, 3, 4, 5, 2, 5, 10, 2 };
            TriangleCase[217] = new byte[] { 2, 0, 4, 2, 4, 10, 10, 4, 5 };
            TriangleCase[218] = new byte[] { 8, 4, 3, 4, 2, 3, 4, 5, 2, 5, 10, 2, 1, 9, 0 };
            TriangleCase[219] = new byte[] { 5, 10, 2, 4, 5, 2, 4, 2, 1, 4, 1, 9 };
            TriangleCase[220] = new byte[] { 3, 8, 1, 8, 4, 1, 4, 5, 1 };
            TriangleCase[221] = new byte[] { 0, 4, 1, 1, 4, 5 };
            TriangleCase[222] = new byte[] { 5, 9, 0, 5, 0, 3, 3, 8, 4, 3, 4, 5 };
            TriangleCase[223] = new byte[] { 4, 5, 9 };
            TriangleCase[224] = new byte[] { 4, 11, 7, 4, 9, 11, 9, 10, 11 };
            TriangleCase[225] = new byte[] { 4, 11, 7, 4, 9, 11, 9, 10, 11, 8, 3, 0 };
            TriangleCase[226] = new byte[] { 0, 1, 4, 1, 10, 4, 10, 7, 4, 10, 11, 7 };
            TriangleCase[227] = new byte[] { 3, 1, 8, 4, 8, 1, 4, 1, 10, 4, 10, 7, 10, 11, 7 };
            TriangleCase[228] = new byte[] { 9, 1, 4, 4, 1, 2, 2, 7, 4, 2, 11, 7 };
            TriangleCase[229] = new byte[] { 9, 1, 4, 4, 1, 2, 2, 7, 4, 2, 11, 7, 0, 8, 3 };
            TriangleCase[230] = new byte[] { 0, 2, 4, 2, 7, 4, 2, 11, 7 };
            TriangleCase[231] = new byte[] { 3, 4, 8, 3, 2, 4, 2, 7, 4, 2, 11, 7 };
            TriangleCase[232] = new byte[] { 4, 9, 10, 4, 10, 7, 2, 7, 10, 2, 3, 7 };
            TriangleCase[233] = new byte[] { 4, 9, 10, 4, 10, 7, 2, 7, 10, 2, 8, 7, 0, 8, 2 };
            TriangleCase[234] = new byte[] { 0, 1, 4, 4, 1, 10, 4, 10, 7, 2, 7, 10, 2, 3, 7 };
            TriangleCase[235] = new byte[] { 8, 7, 4, 2, 1, 10 };
            TriangleCase[236] = new byte[] { 3, 7, 1, 1, 7, 4, 1, 4, 9 };
            TriangleCase[237] = new byte[] { 4, 9, 1, 4, 1, 7, 1, 8, 7, 1, 0, 8 };
            TriangleCase[238] = new byte[] { 3, 7, 0, 7, 4, 0 };
            TriangleCase[239] = new byte[] { 4, 8, 7 };
            TriangleCase[240] = new byte[] { 9, 10, 8, 10, 11, 8 };
            TriangleCase[241] = new byte[] { 3, 0, 9, 3, 9, 10, 3, 10, 11 };
            TriangleCase[242] = new byte[] { 0, 1, 8, 1, 11, 8, 1, 10, 11 };
            TriangleCase[243] = new byte[] { 3, 1, 11, 1, 10, 11 };
            TriangleCase[244] = new byte[] { 8, 9, 1, 8, 1, 2, 8, 2, 11 };
            TriangleCase[245] = new byte[] { 11, 3, 0, 11, 0, 9, 9, 1, 2, 9, 2, 11 };
            TriangleCase[246] = new byte[] { 0, 2, 8, 2, 11, 8 };
            TriangleCase[247] = new byte[] { 3, 2, 11 };
            TriangleCase[248] = new byte[] { 10, 8, 9, 8, 10, 2, 8, 2, 3 };
            TriangleCase[249] = new byte[] { 0, 9, 2, 9, 10, 2 };
            TriangleCase[250] = new byte[] { 0, 1, 8, 8, 1, 10, 8, 10, 2, 8, 2, 3 };
            TriangleCase[251] = new byte[] { 2, 1, 10 };
            TriangleCase[252] = new byte[] { 8, 9, 1, 8, 1, 3 };
            TriangleCase[253] = new byte[] { 0, 9, 1 };
            TriangleCase[254] = new byte[] { 0, 3, 8 };
            TriangleCase[255] = new byte[] { };
        }

        static VoxelCase CoordToCase(VoxelAxe coord)
        {
            switch (coord)
            {
                case VoxelAxe.Xpositive: return FacePoints[3];
                case VoxelAxe.Xnegative: return FacePoints[2];
                case VoxelAxe.Ypositive: return FacePoints[1];
                case VoxelAxe.Ynegative: return FacePoints[4];
                case VoxelAxe.Zpositive: return FacePoints[5];
                case VoxelAxe.Znegative: return FacePoints[0];
                default: return VoxelCase.FULL;
            }
        }

        static VoxelCase CoordToPoint(int xcoord, int ycoord, int zcoord)
        {
            VoxelAxe coord = 0;
            if (xcoord > 0) coord |= VoxelAxe.Xpositive;
            if (xcoord < 0) coord |= VoxelAxe.Xnegative;
            if (ycoord > 0) coord |= VoxelAxe.Ypositive;
            if (ycoord < 0) coord |= VoxelAxe.Ynegative;
            if (zcoord > 0) coord |= VoxelAxe.Zpositive;
            if (zcoord < 0) coord |= VoxelAxe.Znegative;

            switch (coord)
            {
                case VoxelAxe.Xnegative | VoxelAxe.Ynegative | VoxelAxe.Znegative: return VoxelCase.P0;
                case VoxelAxe.Xnegative | VoxelAxe.Ypositive | VoxelAxe.Znegative: return VoxelCase.P1;
                case VoxelAxe.Xpositive | VoxelAxe.Ypositive | VoxelAxe.Znegative: return VoxelCase.P2;
                case VoxelAxe.Xpositive | VoxelAxe.Ynegative | VoxelAxe.Znegative: return VoxelCase.P3;
                case VoxelAxe.Xnegative | VoxelAxe.Ynegative | VoxelAxe.Zpositive: return VoxelCase.P4;
                case VoxelAxe.Xnegative | VoxelAxe.Ypositive | VoxelAxe.Zpositive: return VoxelCase.P5;
                case VoxelAxe.Xpositive | VoxelAxe.Ypositive | VoxelAxe.Zpositive: return VoxelCase.P6;
                case VoxelAxe.Xpositive | VoxelAxe.Ynegative | VoxelAxe.Zpositive: return VoxelCase.P7;
            }
            return VoxelCase.EMPTY;
        }

        static void PointToCoord(VoxelCase point, out int xcoord, out int ycoord, out int zcoord)
        {
            xcoord = ycoord = zcoord = 0;
            switch (point)
            {
                case VoxelCase.P0: xcoord = -1; ycoord = -1; zcoord = -1; break;
                case VoxelCase.P1: xcoord = -1; ycoord = 1; zcoord = -1; break;
                case VoxelCase.P2: xcoord = 1; ycoord = 1; zcoord = -1; break;
                case VoxelCase.P3: xcoord = 1; ycoord = -1; zcoord = -1; break;
                case VoxelCase.P4: xcoord = -1; ycoord = -1; zcoord = 1; break;
                case VoxelCase.P5: xcoord = -1; ycoord = 1; zcoord = 1; break;
                case VoxelCase.P6: xcoord = 1; ycoord = 1; zcoord = 1; break;
                case VoxelCase.P7: xcoord = 1; ycoord = -1; zcoord = 1; break;
            }
        }

        /// <summary>
        /// Remove point not used by neighboar defined by coords
        /// </summary>
        public static VoxelCase FilterUsedByNeighboar(int xcoord, int ycoord, int zcoord, VoxelCase filtercase = VoxelCase.FULL)
        {
            if (xcoord > 0)
            {
                filtercase &= CoordToCase(VoxelAxe.Xpositive);
            }
            if (xcoord < 0)
            {
                filtercase &= CoordToCase(VoxelAxe.Xnegative);
            }
            if (ycoord > 0)
            {
                filtercase &= CoordToCase(VoxelAxe.Ypositive);
            }
            if (ycoord < 0)
            {
                filtercase &= CoordToCase(VoxelAxe.Ynegative);
            }
            if (zcoord > 0)
            {
                filtercase &= CoordToCase(VoxelAxe.Zpositive);
            }
            if (zcoord < 0)
            {
                filtercase &= CoordToCase(VoxelAxe.Znegative);
            }

            return filtercase;
        }

        public static VoxelCase NeighboarConverter(VoxelCase cellcase, int xcoord, int ycoord, int zcoord)
        {
            VoxelCase converted = 0;

            for (int i = 0; i < 8; i++)
            {
                VoxelCase p = cellcase & (VoxelCase)(1 << i);
                if (p != 0)
                {
                    int x, y, z;
                    PointToCoord(p, out x, out y, out z);

                    if (xcoord != 0) x *= -1;
                    if (ycoord != 0) y *= -1;
                    if (zcoord != 0) z *= -1;

                    converted |= CoordToPoint(x, y, z);
                }
            }
            return converted;
        }

        /// <summary>
        /// Get precomputed triangles indices... one week of work !
        /// </summary>
        public static byte[] GetTriangles(VoxelCase cellcase)
        {
            return TriangleCase[(int)cellcase];
        }

        /// <summary>
        /// The 12 vertices in the middle of 12 edges
        /// </summary>
        public static Vector3f[] edgevertex = new Vector3f[]
        {
            new Vector3f(-1,0,-1),
            new Vector3f(0,1,-1),
            new Vector3f(1,0,-1),
            new Vector3f(0,-1,-1),

            new Vector3f(-1,0,1),
            new Vector3f(0,1,1),
            new Vector3f(1,0,1),
            new Vector3f(0,-1,1),

            new Vector3f(-1,-1,0),
            new Vector3f(-1,1,0),
            new Vector3f(1,1,0),
            new Vector3f(1,-1,0)
        };

        /// <summary>
        /// The points of 6 cube's faces
        /// </summary>
        public static VoxelCase[] FacePoints = new VoxelCase[]
        {
            VoxelCase.P0 | VoxelCase.P1 | VoxelCase.P2 | VoxelCase.P3,
            VoxelCase.P1 | VoxelCase.P2 | VoxelCase.P5 | VoxelCase.P6,
            VoxelCase.P0 | VoxelCase.P1 | VoxelCase.P4 | VoxelCase.P5,
            VoxelCase.P2 | VoxelCase.P3 | VoxelCase.P6 | VoxelCase.P7,
            VoxelCase.P0 | VoxelCase.P3 | VoxelCase.P4 | VoxelCase.P7,
            VoxelCase.P4 | VoxelCase.P5 | VoxelCase.P6 | VoxelCase.P7
        };
        /// <summary>
        /// The points of 6 cube's faces
        /// </summary>
        public static VoxelCase[] FaceNeightboarPoints = new VoxelCase[]
        {
            FacePoints[5],
            FacePoints[4],
            FacePoints[3],
            FacePoints[2],
            FacePoints[1],
            FacePoints[0]
        };
        /// <summary>
        /// The 8 cube's corners
        /// </summary>
        public static Vector3f[] cubecorner = new Vector3f[]
        {
            new Vector3f(-1,-1,-1),
            new Vector3f(-1, 1,-1),
            new Vector3f( 1, 1,-1),
            new Vector3f( 1,-1,-1),
            new Vector3f(-1,-1, 1),
            new Vector3f(-1, 1, 1),
            new Vector3f( 1, 1, 1),
            new Vector3f( 1,-1, 1)
        };

        public static Vector3ui[] CornerCoord = new Vector3ui[]
        {
            new Vector3ui(0,0,0),
            new Vector3ui(0,1,0),
            new Vector3ui(1,1,0),
            new Vector3ui(1,0,0),
            new Vector3ui(0,0,1),
            new Vector3ui(0,1,1),
            new Vector3ui(1,1,1),
            new Vector3ui(1,0,1)
        };


        /// <summary>
        /// The lines list of cube's edges
        /// </summary>
        public static ushort[] cubeedges = new ushort[] 
        { 
            0, 1, 1, 2, 2, 3, 3, 0, 4, 5, 5, 6, 6, 7, 7, 4, 0, 4, 1, 5, 2, 6, 3, 7
        };

    }

}
