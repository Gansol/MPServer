using MPProtocol;

/// <summary>
/// use array best for speed.
/// </summary>
public static class SpawnData
{
    public static object GetSpawnData(SpawnStatus status)
    {
        object spawnData = "";

        switch (status)
        {
            case SpawnStatus.LineL:
            case SpawnStatus.ReLineL:
            case SpawnStatus.Random:
                spawnData = aLineL;
                break;
            case SpawnStatus.LineR:
            case SpawnStatus.ReLineR:
                spawnData = aLineR;
                break;
            case SpawnStatus.LinkLineL:
            case SpawnStatus.ReLinkLineL:
                spawnData = aLinkLineL;
                break;
            case SpawnStatus.LinkLineR:
            case SpawnStatus.ReLinkLineR:
                spawnData = aLinkLineL;
                break;
            case SpawnStatus.CircleLD:
            case SpawnStatus.ReCircleLD:
                spawnData = aCircleLD;
                break;
            case SpawnStatus.CircleRU:
            case SpawnStatus.ReCircleRU:
                spawnData = aCircleRU;
                break;
            case SpawnStatus.VerticalL:
            case SpawnStatus.ReVerticalL:
                spawnData = aVertL2D;
                break;
            case SpawnStatus.VerticalR:
            case SpawnStatus.ReVerticalR:
                spawnData = aVertR2D;
                break;
            case SpawnStatus.LinkVertL:
            case SpawnStatus.ReLinkVertL:
                spawnData = aLinkVertL2D;
                break;
            case SpawnStatus.LinkVertR:
            case SpawnStatus.ReLinkVertR:
                spawnData = aLinkVertR2D;
                break;
            case SpawnStatus.HorizontalD:
            case SpawnStatus.ReHorizontalD:
                spawnData = aHorD2D;
                break;
            case SpawnStatus.HorizontalU:
            case SpawnStatus.ReHorizontalU:
                spawnData = aHorU2D;
                break;
            case SpawnStatus.LinkHorD:
            case SpawnStatus.ReLinkHorD:
                spawnData = aLinkHorD2D;
                break;
            case SpawnStatus.LinkHorU:
            case SpawnStatus.ReLinkHorU:
                spawnData = aLinkHorU2D;
                break;
            case SpawnStatus.HorTwin:
            case SpawnStatus.ReHorTwin:
                spawnData = aHorTwin2D;
                break;
            case SpawnStatus.VertTwin:
            case SpawnStatus.ReVertTwin:
                spawnData = aVertTwin2D;
                break;
            case SpawnStatus.LinkHorTwin:
            case SpawnStatus.ReLinkHorTwin:
                spawnData = aLinkHorD2D;
                break;
            case SpawnStatus.LinkVertTwin:
            case SpawnStatus.ReLinkVertTwin:
                spawnData = aLinkVertTwin2D;
                break;
            case SpawnStatus.TriangleLD:
            case SpawnStatus.ReTriangleLD:
                spawnData = jaTriangleLD2D;
                break;
            case SpawnStatus.TriangleLU:
            case SpawnStatus.ReTriangleLU:
                spawnData = jaTriangleLU2D;
                break;
            case SpawnStatus.TriangleRD:
            case SpawnStatus.ReTriangleRD:
                spawnData = jaTriangleRD2D;
                break;
            case SpawnStatus.TriangleRU:
            case SpawnStatus.ReTriangleRU:
                spawnData = jaTriangleRU2D;
                break;
            case SpawnStatus.BevelL:
            case SpawnStatus.ReBevelL:
                spawnData = jaBevelL2D;
                break;
            case SpawnStatus.BevelR:
            case SpawnStatus.ReBevelR:
                spawnData = jaBevelR2D;
                break;

            case SpawnStatus.FourPoint:
                spawnData = aFourPoint;
                break;
        }
        return spawnData;
    }


    private static readonly sbyte[] aLineL = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
    private static readonly sbyte[] aLineR = { 3, 2, 1, 6, 5, 4, 9, 8, 7, 12, 11, 10 };

    private static readonly sbyte[] aLinkLineL = { 1, 2, 3, 6, 5, 4, 7, 8, 9, 12, 11, 10 };
    private static readonly sbyte[] aLinkLineR = { 3, 2, 1, 4, 5, 6, 9, 8, 7, 10, 11, 12 };

    private static readonly sbyte[] aCircleLD = { 1, 4, 7, 10, 11, 12, 9, 6, 3, 2, 5, 8 };
    private static readonly sbyte[] aCircleRU = { 12, 9, 6, 3, 2, 1, 4, 7, 10, 11, 8, 5 };

    private static readonly sbyte[] aFourPoint = { 10, 12, 1, 3 };

    private static readonly sbyte[,] aVertL2D = { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 }, { 10, 11, 12 } };
    private static readonly sbyte[,] aVertR2D = { { 3, 2, 1 }, { 6, 5, 4 }, { 9, 8, 7 }, { 12, 11, 10 } };

    private static readonly sbyte[,] aLinkVertL2D = { { 1, 2, 3 }, { 6, 5, 4 }, { 7, 8, 9 }, { 12, 11, 10 } };
    private static readonly sbyte[,] aLinkVertR2D = { { 3, 2, 1 }, { 4, 5, 6 }, { 9, 8, 7 }, { 10, 11, 12 } };

    private static readonly sbyte[,] aHorD2D = { { 1, 4, 7, 10 }, { 2, 5, 8, 11 }, { 3, 6, 9, 12 } };
    private static readonly sbyte[,] aHorU2D = { { 10, 7, 4, 1 }, { 11, 8, 5, 2 }, { 12, 9, 6, 3 } };

    private static readonly sbyte[,] aLinkHorD2D = { { 1, 4, 7, 10 }, { 11, 8, 5, 2 }, { 3, 6, 9, 12 } };
    private static readonly sbyte[,] aLinkHorU2D = { { 12, 9, 6, 3 }, { 2, 5, 8, 11 }, { 10, 7, 4, 1 } };

    private static readonly sbyte[,] aHorTwin2D = { { 1, 4 }, { 2, 5 }, { 3, 6 }, { 7, 10 }, { 8, 11 }, { 9, 12 } };
    private static readonly sbyte[,] aVertTwin2D = { { 1, 2 }, { 4, 5 }, { 3, 6 }, { 9, 8 }, { 12, 11 }, { 10, 7 } };

    private static readonly sbyte[,] aLinkHorTwin2D = { { 1, 4 }, { 5, 2 }, { 3, 6 }, { 12, 9 }, { 8, 11 }, { 10, 7 } };
    private static readonly sbyte[,] aLinkVertTwin2D = { { 2, 1 }, { 4, 5 }, { 8, 7 }, { 10, 11 }, { 12, 9 }, { 6, 3 } };

    /// <summary>
    /// 左下3角型
    /// </summary>
    private static readonly sbyte[][] jaTriangleLD2D = new sbyte[][]{
        new sbyte[] {1,4,7},
        new sbyte[] {2,5},
        new sbyte[] {3},
        //new sbyte[] {11,12,9}
    };

    /// <summary>
    /// 右下3角型
    /// </summary>
    private static readonly sbyte[][] jaTriangleRD2D = new sbyte[][]{
        new sbyte[] {3,6,9},
        new sbyte[] {2,5},
        new sbyte[] {1},
         //new sbyte[] {7,10,11}
    };

    /// <summary>
    /// 左上3角型
    /// </summary>
    private static readonly sbyte[][] jaTriangleLU2D = new sbyte[][]{
        new sbyte[] {10,7,4},
        new sbyte[] {11,8},
        new sbyte[] {12},
        //new sbyte[] {2,3,6}
    };

    /// <summary>
    /// 右上3角型
    /// </summary>
    private static readonly sbyte[][] jaTriangleRU2D = new sbyte[][]{
        new sbyte[] {12,9,6},
        new sbyte[] {11,8},
        new sbyte[] {10},
        //new sbyte[] {1,2,4}
    };

    /// <summary>
    /// 左邊斜角
    /// </summary>
    private static readonly sbyte[][] jaBevelL2D = new sbyte[][]{
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
    private static readonly sbyte[][] jaBevelR2D = new sbyte[][]{
        new sbyte[] {12},
        new sbyte[] {11,9},
        new sbyte[] {10,8,6},
        new sbyte[] {7,5,3},
        new sbyte[] {4,2},
        new sbyte[] {1}
    };
}
