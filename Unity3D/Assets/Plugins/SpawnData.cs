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
            case SpawnStatus.Random:
            default:
                spawnData = aLineL;
                break;
            case SpawnStatus.LineR:
                spawnData = aLineR;
                break;
            case SpawnStatus.LineHorA:
                spawnData = aLineHorA;
                break;
            case SpawnStatus.LineHorB:
                spawnData = aLineHorB;
                break;
            case SpawnStatus.LineHorC:
                spawnData = aLineHorC;
                break;
            case SpawnStatus.LineHorD:
                spawnData = aLineHorD;
                break;
            case SpawnStatus.LineVertA:
                spawnData = aLineVertA;
                break;
            case SpawnStatus.LineVertB:
                spawnData = aLineVertB;
                break;
            case SpawnStatus.LineVertC:
                spawnData = aLineVertC;
                break;
            case SpawnStatus.LinkLineL:
                spawnData = aLinkLineL;
                break;
            case SpawnStatus.LinkLineR:
                spawnData = aLinkLineR;
                break;
            case SpawnStatus.CircleLD:
                spawnData = aCircleLD;
                break;
            case SpawnStatus.CircleRU:
                spawnData = aCircleRU;
                break;
            case SpawnStatus.BevelL:
                spawnData = aBevelL;
                break;
            case SpawnStatus.BevelR:
                spawnData = aBevelR;
                break;
            case SpawnStatus.VerticalL:
                spawnData = aVertL2D;
                break;
            case SpawnStatus.VerticalR:
                spawnData = aVertR2D;
                break;
            case SpawnStatus.LinkVertL:
                spawnData = aLinkVertL2D;
                break;
            case SpawnStatus.LinkVertR:
                spawnData = aLinkVertR2D;
                break;
            case SpawnStatus.HorizontalD:
                spawnData = aHorD2D;
                break;
            case SpawnStatus.HorizontalU:
                spawnData = aHorU2D;
                break;
            case SpawnStatus.LinkHorD:
                spawnData = aLinkHorD2D;
                break;
            case SpawnStatus.LinkHorU:
                spawnData = aLinkHorU2D;
                break;
            case SpawnStatus.HorTwin:
                spawnData = aHorTwin2D;
                break;
            case SpawnStatus.VertTwin:
                spawnData = aVertTwin2D;
                break;
            case SpawnStatus.STwin:
                spawnData = aSTwin;
                break;
            //case SpawnStatus.TriangleLD:
            //    spawnData = jaTriangleLD2D;
            //    break;
            //case SpawnStatus.TriangleLU:
            //    spawnData = jaTriangleLU2D;
            //    break;
            //case SpawnStatus.TriangleRD:
            //    spawnData = jaTriangleRD2D;
            //    break;
            //case SpawnStatus.TriangleRU:
            //    spawnData = jaTriangleRU2D;
            //    break;


            case SpawnStatus.FourPoint:
                spawnData = aFourPoint;
                break;
        }
        return spawnData;
    }

    private static readonly sbyte[] aLineHorA = { 0, 1, 2 };
    private static readonly sbyte[] aLineHorB = { 3, 4, 5 };
    private static readonly sbyte[] aLineHorC = { 6, 7, 8 };
    private static readonly sbyte[] aLineHorD = { 9, 10, 11 };

    private static readonly sbyte[] aLineVertA = { 0, 3, 6, 9 };
    private static readonly sbyte[] aLineVertB = { 1, 4, 7, 10 };
    private static readonly sbyte[] aLineVertC = { 2, 5, 8, 11 };


    private static readonly sbyte[] aLineL = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
    private static readonly sbyte[] aLineR = { 2, 1, 0, 5, 4, 3, 8, 7, 6, 11, 10, 9 };



    private static readonly sbyte[] aLinkLineL = { 0, 1, 2, 5, 4, 3, 6, 7, 8, 11, 10, 9 };
    private static readonly sbyte[] aLinkLineR = { 2, 1, 0, 3, 4, 5, 8, 7, 6, 9, 10, 11 };

    private static readonly sbyte[] aBevelL = { 0, 1, 3, 2, 4, 6, 5, 7, 9, 8, 10, 11 };
    private static readonly sbyte[] aBevelR = { 2, 1, 5, 0, 4, 8, 3, 7, 11, 6, 10, 9 };

    private static readonly sbyte[] aCircleLD = { 0, 3, 6, 9, 10, 11, 8, 5, 2, 1, 4, 7 };
    private static readonly sbyte[] aCircleRU = { 11, 8, 5, 2, 1, 0, 3, 6, 9, 10, 7, 4 };

    private static readonly sbyte[] aSTwin = { 0, 3, 4, 1, 2, 5, 11, 8, 7, 10, 9, 6 };
    private static readonly sbyte[] aFourPoint = { 9, 11, 0, 2 };

    private static readonly sbyte[,] aVertL2D = { { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 }, { 9, 10, 11 } };
    private static readonly sbyte[,] aVertR2D = { { 2, 1, 0 }, { 5, 4, 3 }, { 8, 7, 6 }, { 11, 10, 9 } };

    private static readonly sbyte[,] aLinkVertL2D = { { 0, 1, 2 }, { 5, 4, 3 }, { 6, 7, 8 }, { 11, 10, 9 } };
    private static readonly sbyte[,] aLinkVertR2D = { { 2, 1, 0 }, { 3, 4, 5 }, { 8, 7, 6 }, { 9, 10, 11 } };

    private static readonly sbyte[,] aHorD2D = { { 0, 3, 6, 9 }, { 1, 4, 7, 10 }, { 2, 5, 8, 11 } };
    private static readonly sbyte[,] aHorU2D = { { 9, 6, 3, 0 }, { 10, 7, 4, 1 }, { 11, 8, 5, 2 } };

    private static readonly sbyte[,] aLinkHorD2D = { { 0, 3, 6, 9 }, { 10, 7, 4, 1 }, { 2, 5, 8, 11 } };
    private static readonly sbyte[,] aLinkHorU2D = { { 11, 8, 5, 2 }, { 1, 4, 7, 10 }, { 9, 6, 3, 0 } };

    private static readonly sbyte[,] aHorTwin2D = { { 0, 3 }, { 1, 4 }, { 2, 5 }, { 6, 9 }, { 7, 10 }, { 8, 11 } };
    private static readonly sbyte[,] aVertTwin2D = { { 0, 1 }, { 3, 4 }, { 2, 5 }, { 8, 7 }, { 11, 10 }, { 9, 6 } };

    
   // private static readonly sbyte[,] aSnakeCircle2D = { { 1, 0 }, { 3, 4 }, { 7, 6 }, { 9, 10 }, { 11, 8 }, { 5, 2 } };


    ///// <summary>
    ///// 左下3角型
    ///// </summary>
    //private static readonly sbyte[][] jaTriangleLD2D = new sbyte[][]{
    //    new sbyte[] {0,3,6},
    //    new sbyte[] {1,4},
    //    new sbyte[] {2},
    //    //new sbyte[] {11,12,9}
    //};

    ///// <summary>
    ///// 右下3角型
    ///// </summary>
    //private static readonly sbyte[][] jaTriangleRD2D = new sbyte[][]{
    //    new sbyte[] {2,5,8},
    //    new sbyte[] {1,4},
    //    new sbyte[] {0},
    //     //new sbyte[] {7,10,11}
    //};

    ///// <summary>
    ///// 左上3角型
    ///// </summary>
    //private static readonly sbyte[][] jaTriangleLU2D = new sbyte[][]{
    //    new sbyte[] {9,6,3},
    //    new sbyte[] {10,7},
    //    new sbyte[] {11},
    //    //new sbyte[] {2,3,6}
    //};

    ///// <summary>
    ///// 右上3角型
    ///// </summary>
    //private static readonly sbyte[][] jaTriangleRU2D = new sbyte[][]{
    //    new sbyte[] {11,8,5},
    //    new sbyte[] {10,7},
    //    new sbyte[] {9},
    //    //new sbyte[] {1,2,4}
    //};

    ///// <summary>
    ///// 左邊斜角
    ///// </summary>
    //private static readonly sbyte[][] jaBevelL2D = new sbyte[][]{
    //    new sbyte[] {9},
    //    new sbyte[] {6,10},
    //    new sbyte[] {3,7,11},
    //    new sbyte[] {0,4,8},
    //    new sbyte[] {1,5},
    //    new sbyte[] {2}
    //};

    ///// <summary>
    ///// 右邊斜角
    ///// </summary>
    //private static readonly sbyte[][] jaBevelR2D = new sbyte[][]{
    //    new sbyte[] {11},
    //    new sbyte[] {10,8},
    //    new sbyte[] {9,7,5},
    //    new sbyte[] {6,4,2},
    //    new sbyte[] {3,1},
    //    new sbyte[] {0}
    //};
}
