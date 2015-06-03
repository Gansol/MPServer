

public static class SpawnData
{
    public static readonly sbyte[] aLineL = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
    public static readonly sbyte[] aLineR = { 3, 2, 1, 6, 5, 4, 9, 8, 7, 12, 11, 10 };

    public static readonly sbyte[] aLinkLineL = { 1, 2, 3, 6, 5, 4, 7, 8, 9, 12, 11, 10 };
    public static readonly sbyte[] aLinkLineR = { 3, 2, 1, 4, 5, 6, 9, 8, 7, 10, 11, 12 };

    public static readonly sbyte[] aCircleLD = { 1, 4, 7, 10, 11, 12, 9, 6, 3, 2, 5, 8 };
    public static readonly sbyte[] aCircleRU = { 12, 9, 6, 3, 2, 1, 4, 7, 10, 11, 8, 5 };


    public static readonly sbyte[,] aVertL2D = { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 }, { 10, 11, 12 } };
    public static readonly sbyte[,] aVertR2D = { { 3, 2, 1 }, { 6, 5, 4 }, { 9, 8, 7 }, { 12, 11, 10 } };

    public static readonly sbyte[,] aLinkVertL2D = { { 1, 2, 3 }, { 6, 5, 4 }, { 7, 8, 9 }, { 12, 11, 10 } };
    public static readonly sbyte[,] aLinkVertR2D = { { 3, 2, 1 }, { 4, 5, 6 }, { 9, 8, 7 }, { 10, 11, 12 } };

    public static readonly sbyte[,] aHorD2D = { { 1, 4, 7, 10 }, { 2, 5, 8, 11 }, { 3, 6, 9, 12 } };
    public static readonly sbyte[,] aHorU2D = { { 10, 7, 4, 1 }, { 11, 8, 5, 2 }, { 12, 9, 6, 3 } };

    public static readonly sbyte[,] aLinkHorD2D = { { 1, 4, 7, 10 }, { 11, 8, 5, 2 }, { 3, 6, 9, 12 } };
    public static readonly sbyte[,] aLinkHorU2D = { { 10, 7, 4, 1 }, { 2, 5, 8, 11 }, { 12, 9, 6, 3 } };

    public static readonly sbyte[,] aHorTwin2D = { { 1, 4 }, { 2, 5 }, { 3, 6 }, { 4, 7 }, { 5, 8 }, { 6, 9 }, { 7, 10 }, { 8, 11 }, { 9, 12 } };
    public static readonly sbyte[,] aVertTwin2D = { { 1, 2 }, { 2, 3 }, { 4, 5 }, { 5, 6 }, { 7, 8 }, { 8, 9 }, { 10, 11 }, { 11, 12 } };

    public static readonly sbyte[,] aLinkHorTwin2D = { { 1, 4 }, { 5, 2 }, { 3, 6 }, { 9, 9 }, { 8, 5 }, { 4, 7 }, { 7, 10 }, { 11, 8 }, { 9, 12 } };
    public static readonly sbyte[,] aLinkVertTwin2D = { { 1, 2 }, { 3, 6 }, { 5, 4 }, { 7, 8 }, { 9, 12 }, { 11, 10 } };

    /// <summary>
    /// 左下3角型
    /// </summary>
    public static readonly sbyte[][] jaTriangleLD2D = new sbyte[][]{
        new sbyte[] {1,4,7},
        new sbyte[] {2,5},
        new sbyte[] {3},
        new sbyte[] {11,12,9}
    };

    /// <summary>
    /// 右下3角型
    /// </summary>
    public static readonly sbyte[][] jaTriangleRD2D = new sbyte[][]{
        new sbyte[] {3,6,9},
        new sbyte[] {2,5},
        new sbyte[] {1},
         new sbyte[] {7,10,11}
    };

    /// <summary>
    /// 左上3角型
    /// </summary>
    public static readonly sbyte[][] jaTriangleLU2D = new sbyte[][]{
        new sbyte[] {10,7,4},
        new sbyte[] {11,8},
        new sbyte[] {12},
        new sbyte[] {2,3,6}
    };

    /// <summary>
    /// 右上3角型
    /// </summary>
    public static readonly sbyte[][] jaTriangleRU2D = new sbyte[][]{
        new sbyte[] {12,9,6},
        new sbyte[] {11,8},
        new sbyte[] {10},
        new sbyte[] {1,2,4}
    };

    /// <summary>
    /// 左邊斜角
    /// </summary>
    public static readonly sbyte[][] jaBevelL2D = new sbyte[][]{
        new sbyte[] {10},
        new sbyte[] {7,11},
        new sbyte[] {4,8,12},
        new sbyte[] {1,5,9},
        new sbyte[] {2,6},
        new sbyte[] {3}
    };

    /// <summary>
    /// 右邊斜角
    /// </summary>
    public static readonly sbyte[][] jaBevelR2D = new sbyte[][]{
        new sbyte[] {12},
        new sbyte[] {11,9},
        new sbyte[] {10,8,6},
        new sbyte[] {7,5,3},
        new sbyte[] {4,2},
        new sbyte[] {1}
    };
}
