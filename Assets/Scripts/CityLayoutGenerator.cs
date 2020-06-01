using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityLayoutGenerator : MonoBehaviour
{
    private GameObject[] DistrictGameObjects;
    private Transform DistrictMarkers;

    public int NeighborhoodMinimumWidth = 10;
    public int NeighborhoodMinimumBreadth = 10;

    public int NeighborhoodMaximumWidth = 40;
    public int NeighborhoodMaximumBreadth = 30;

    public int RoadMinUnitWidth = 2;
    public int RoadMaxUnitWidth = 3;

    [Range(0, 1.0f)] public float StopDividingAfterSmallerThanMaximumAreaChance = 0.5f;


    public const int minimumWidth = 4;
    public const int minimumBreadth = 4;

    private int minimumVerticalCut;
    private int minimumHorizontalCut;

    [Range(4, 500)] public int Width;
    [Range(4, 500)] public int Breadth;




    private static float GizmoCircleRadius = 2f;

    public int perUnitMultiplier = 3;
    [Range(0, 100)] public int seed;


    private ArchitecturalStylesArray BuildingUnitArray;

   
    // Start is called before the first frame update
    public void ResetBuild()
    {
        //Destroy(buildGenerator);
        
        while (transform.childCount > 0)
        {
            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject);
            }
        }


    }

    public void GenerateBuildings()
    {
        Debug.Log("GenerateBuildings");
        foreach(Transform child in transform)
        {
            BuildArea2D buildArea = child.GetComponent<BuildArea2D>();
            if (buildArea)
            {
                
                BuildingGenerator BG = buildArea.StartGenerate();
                BG.generate();
            }
        }
    }

    void Start()
    {


        //Init();
    }

    public void Init()
    {
        BuildingUnitArray = GameObject.FindGameObjectWithTag("Architecture").GetComponent<ArchitecturalStylesArray>();

        if (!BuildingUnitArray)
        {
            var x = Resources.FindObjectsOfTypeAll(typeof(ArchitecturalStylesArray));
            BuildingUnitArray = (ArchitecturalStylesArray)x[0];

        }
        BuildingUnitArray.Init();
        GenerateLayout();

        GenerateNeighborhood(transform);

        GenerateRoads(transform);
    }

    void GenerateLayout()
    {
        
        


        minimumVerticalCut = NeighborhoodMinimumBreadth * 2 + RoadMinUnitWidth;
        minimumHorizontalCut = NeighborhoodMinimumWidth * 2 + RoadMinUnitWidth;

        Random.InitState(seed);
        //Generate initial Shape2D
        Shape2D initShape;
        Utils.SpawnShape(transform, GetSquareVertices(), transform.position, (int)LayoutGenerationOpt.HorizontalDivide,out initShape);


        List<Shape2D> shapesFound = new List<Shape2D>();

        bool ExistsShapeWithBuildInstructions = true;
        

        while (ExistsShapeWithBuildInstructions)
        {
            foreach (Transform child in transform)
            {
                Shape2D Shape = child.gameObject.GetComponent<Shape2D>();
                if (Shape && Shape.Identifier != (int)LayoutGenerationOpt.Road && Shape.Identifier != (int)LayoutGenerationOpt.NoBuild)
                {
                    shapesFound.Add(Shape);
                }

            }

            ExistsShapeWithBuildInstructions = shapesFound.Count != 0;

            foreach (Shape2D shape in shapesFound)
            {
                switch ((LayoutGenerationOpt)shape.Identifier)
                {
                    case LayoutGenerationOpt.HorizontalDivide:
                        DivideHorinzontal(transform,shape,perUnitMultiplier);
                        break;

                    case LayoutGenerationOpt.VerticalDivide:
                        DivideVertical(transform, shape, perUnitMultiplier);
                        break;

                    case LayoutGenerationOpt.Road:
                        break;

                }

            }

            shapesFound.Clear();
        }




    }

    void DivideVertical(Transform transform, Shape2D shape, int perUnitMultiplier)
    {


        int currentUnitBreadth = Utils.GetPerUnitLength(shape.vertices[0], shape.vertices[3], perUnitMultiplier);
        int currentUnitWidth = Utils.GetPerUnitLength(shape.vertices[0], shape.vertices[1], perUnitMultiplier);

        if (currentUnitWidth < NeighborhoodMaximumWidth && currentUnitBreadth < NeighborhoodMaximumBreadth)
        {
            float decision = Random.Range(0, 1.0f);

            if (decision < StopDividingAfterSmallerThanMaximumAreaChance)
            {
                return;
            }
        }

        bool CanCutInHorizontal = currentUnitBreadth >= NeighborhoodMinimumBreadth * 2 + RoadMinUnitWidth;
        bool CanCutVertical = currentUnitWidth >= NeighborhoodMinimumWidth * 2 + RoadMinUnitWidth;


        Shape2D otherShape = null;

        if (CanCutVertical)
        {
            CutVertical(shape, currentUnitWidth, otherShape);

        }
        else if (CanCutInHorizontal)
        {
            
            CutHorizontal(shape, currentUnitBreadth, otherShape);
        }
        else
        {
            shape.Identifier = (int)LayoutGenerationOpt.NoBuild;
        }
    }

    void DivideHorinzontal(Transform transform, Shape2D shape,int perUnitMultiplier)
    {

        int currentUnitBreadth = Utils.GetPerUnitLength(shape.vertices[0], shape.vertices[3], perUnitMultiplier);
        int currentUnitWidth = Utils.GetPerUnitLength(shape.vertices[0], shape.vertices[1], perUnitMultiplier);

        if (currentUnitWidth < NeighborhoodMaximumWidth && currentUnitBreadth < NeighborhoodMaximumBreadth)
        {
            float decision = Random.Range(0, 1.0f);

            if (decision < StopDividingAfterSmallerThanMaximumAreaChance)
            {
                return;
            }
        }


        bool CanCutInHorizontal = currentUnitBreadth >= NeighborhoodMinimumBreadth * 2 + RoadMinUnitWidth;
        bool CanCutVertical = currentUnitWidth >= NeighborhoodMinimumWidth * 2 + RoadMinUnitWidth;



        Shape2D otherShape = null;

        if(CanCutInHorizontal)
        {
            CutHorizontal( shape, currentUnitBreadth,  otherShape);

        }
        else if(CanCutVertical)
        {
            CutVertical(shape, currentUnitWidth,  otherShape);

        }
        else
        {
            shape.Identifier = (int)LayoutGenerationOpt.NoBuild;
        }
       
        //Utils.SliceSquareShape2D(shape, true, perUnitMultiplier, transform, transform, out otherShape);
    }

    void CutVertical(Shape2D shape, int currentUnitWidth,Shape2D otherShape)
    {
        int newSize = Random.Range(NeighborhoodMinimumWidth, (currentUnitWidth + 1) - NeighborhoodMinimumWidth);
        Utils.SliceSquareShape2D(shape, false, perUnitMultiplier, newSize, transform, out otherShape);

        int maxRoadWidth = (currentUnitWidth - minimumVerticalCut) < RoadMaxUnitWidth ? (currentUnitWidth - minimumVerticalCut) : RoadMaxUnitWidth;
        int roadWidth = Random.Range(RoadMinUnitWidth, maxRoadWidth);

        ///Shrink Shape2Ds For Road

        int shrinkAmountForRoad = (newSize - minimumVerticalCut) > 0 ? Mathf.Clamp(newSize - minimumVerticalCut, 0, roadWidth) : 0;
        Utils.ResizeSquareShape2DOnRight(shape, shrinkAmountForRoad, -transform.right, perUnitMultiplier);

        Utils.ResizeSquareShape2DOnLeft(otherShape, roadWidth - shrinkAmountForRoad, transform.right, perUnitMultiplier);

        //
        Vector3[] RoadVertices = new Vector3[4];

        RoadVertices[0] = otherShape.vertices[1];
        RoadVertices[1] = shape.vertices[0];
        RoadVertices[2] = shape.vertices[3];
        RoadVertices[3] = otherShape.vertices[2];

        Shape2D RoadVertex;
        Utils.SpawnShape(transform, RoadVertices, transform.position, (int)LayoutGenerationOpt.Road, out RoadVertex);
        otherShape.Identifier = (int)LayoutGenerationOpt.HorizontalDivide;
        shape.Identifier = (int)LayoutGenerationOpt.HorizontalDivide;

        RoadVertex.VertexCrossDisplay = true;
    }

    void CutHorizontal(Shape2D shape,int currentUnitBreadth,Shape2D otherShape)
    {
        int newSize = Random.Range(NeighborhoodMinimumBreadth, (currentUnitBreadth + 1) - NeighborhoodMinimumBreadth);
        Utils.SliceSquareShape2D(shape, true, perUnitMultiplier, newSize, transform, out otherShape);

        int maxRoadWidth = (currentUnitBreadth - minimumHorizontalCut) < RoadMaxUnitWidth ? (currentUnitBreadth - minimumHorizontalCut) : RoadMaxUnitWidth;
        int roadWidth = Random.Range(RoadMinUnitWidth, maxRoadWidth);

        ///Shrink Shape2Ds For Road

        int shrinkAmountForRoad = (newSize - minimumHorizontalCut) > 0 ? Mathf.Clamp(newSize - minimumHorizontalCut, 0, roadWidth) : 0;
        Utils.ResizeSquareShape2DOnBack(shape, shrinkAmountForRoad, transform.forward, perUnitMultiplier);

        Utils.ResizeSquareShape2DOnFront(otherShape, roadWidth - shrinkAmountForRoad, -transform.forward, perUnitMultiplier);

        //
        Vector3[] RoadVertices = new Vector3[4];

        RoadVertices[0] = otherShape.vertices[0];
        RoadVertices[1] = shape.vertices[3];
        RoadVertices[2] = shape.vertices[2];
        RoadVertices[3] = otherShape.vertices[1];

        Shape2D RoadVertex;
        Utils.SpawnShape(transform, RoadVertices, transform.position, (int)LayoutGenerationOpt.Road, out RoadVertex);
        otherShape.Identifier = (int)LayoutGenerationOpt.VerticalDivide;
        shape.Identifier = (int)LayoutGenerationOpt.VerticalDivide;

        otherShape.GizmoCircleRadius = 2.0f;
        shape.GizmoCircleRadius = 2.0f;
        RoadVertex.VertexCrossDisplay = true;
    }


    void GenerateNeighborhood(Transform transform)
    {

        DistrictMarkers = GameObject.FindGameObjectWithTag("Districts").transform;

        List<DistrictMarker> districtMarkerList = new List<DistrictMarker>();

        foreach (Transform child in DistrictMarkers)
        {
            DistrictMarker marker = child.gameObject.GetComponent<DistrictMarker>();

            if (marker)
            {
                districtMarkerList.Add(marker);
            }

        }


        //for each shape with "noBuild"
        List<Shape2D> BuildShapes = new List<Shape2D>();

        Utils.FindShape2DInTransform(transform, BuildShapes, (int)LayoutGenerationOpt.Road);

        Debug.Log("BuildShapes " + BuildShapes.Count);

        List<Neighborhood> Neighborhoods = new List<Neighborhood>();

        Debug.Log("ShapeCount " + BuildShapes.Count);
        foreach (Shape2D shape in BuildShapes)
        {
            GameObject gameObject = shape.gameObject;

            Neighborhood neighborhood = gameObject.AddComponent<Neighborhood>();
            VoronoiSetDistrict(neighborhood, districtMarkerList);
            Neighborhoods.Add(neighborhood);

            if(shape == null) { Debug.Log("NULL SHAPE"); }
            gameObject.AddComponent<RoadTextureGenerator>().CreateMesh(shape.vertices, BuildingUnitArray.groundTexture, 0.25f, true,transform);


        }


        foreach (Shape2D shape in BuildShapes)
        {
            shape.TurnOffAllGizmos = false;
            GenerateBuildAreas(shape);


        }
    }

    void GenerateRoads(Transform transform)
    {
        List<Shape2D> roads = new List<Shape2D>();

        Utils.FindShape2DInTransformOfType((int)LayoutGenerationOpt.Road, transform, roads);

        foreach(Shape2D shape in roads)
        {
            RoadTextureGenerator RoadGenerator = shape.gameObject.AddComponent<RoadTextureGenerator>();
            RoadGenerator.CreateMesh(shape.vertices, BuildingUnitArray.roadTexture, 0.8f,false,transform);
        }

    }

    public void GenerateBuildAreas(Shape2D shape)
    {
        //get width
        int width = Utils.GetPerUnitLength(shape.vertices[0], shape.vertices[1], perUnitMultiplier);
        int breadth = Utils.GetPerUnitLength(shape.vertices[0], shape.vertices[3], perUnitMultiplier);

        Vector2 MinSize, MaxSize;

        Neighborhood neighborhood = shape.GetComponent<Neighborhood>();

        //if (
        //       neighborhood.district == District.Entropism
        //    || neighborhood.district == District.NeoKitsch
        //    || neighborhood.district == District.Kitsch
        //    || neighborhood.district == District.NeoMilitarism
        //)
        //{
        //    return;
        //}

        GetDistrictPrefferedSize(neighborhood, out MinSize, out MaxSize);
        //get breadth
        //get preffered sizes

        if(width < MinSize.x || breadth < MinSize.y  ) { return; }
        //Debug.Log("MinSize.x > widtht");
        //-----------------Make buildings on left row 

        int WidthSpaceUsed;

        List<Vector2> BuildingSizesLeft = GenerateSideBuildingSizes(width, breadth, MaxSize, MinSize,out WidthSpaceUsed, neighborhood.district);

        GenerateSideNeighborhood(shape.vertices[1],  BuildingSizesLeft, neighborhood.district,1);
        //Make Buildings on right row

        if(width - WidthSpaceUsed < MinSize.x)
        {
            return;
        }

        int WidthSpaceUsedForRight;
        List<Vector2> BuildingSizesRight = GenerateSideBuildingSizes(width - WidthSpaceUsed, breadth, MaxSize, MinSize,out WidthSpaceUsedForRight, neighborhood.district);

        GenerateSideNeighborhood(shape.vertices[0], BuildingSizesRight, neighborhood.district, -1);

        int widthLeft = width - WidthSpaceUsed - WidthSpaceUsedForRight;

        if (widthLeft < MinSize.x)
        {
            return;
        }

        int BreadthSpaceUsed;
        List<Vector2> BuildingSizeFront = GenerateFrontalBuildingSizes(widthLeft, breadth, MaxSize, MinSize, out BreadthSpaceUsed, neighborhood.district);
        //GenerateSideNeighborhood(shape.vertices[1] + WidthSpaceUsed * transform.right, BuildingSizesRight, neighborhood.district, -1);

        GenerateFrontalNeighborhood(shape.vertices[1] + WidthSpaceUsed*transform.right*perUnitMultiplier, BuildingSizeFront, neighborhood.district, 1);


        if(breadth - BreadthSpaceUsed < MinSize.y)
        {
            return;
        }

        int BreadthSpaceUsedForBack;
        List<Vector2> BuildingSizeBack = GenerateFrontalBuildingSizes(widthLeft, breadth - BreadthSpaceUsed, MaxSize, MinSize, out BreadthSpaceUsedForBack, neighborhood.district);

        GenerateFrontalNeighborhood(shape.vertices[2] + WidthSpaceUsed * transform.right * perUnitMultiplier, BuildingSizeFront, neighborhood.district, -1);

    }

    private List<Vector2> GenerateFrontalBuildingSizes(int width, int breadth, Vector2 MaxSize, Vector2 MinSize, out int BreadthSpaceUsed,District district)
    {
        List<Vector2> BuildingSizesLeft = new List<Vector2>();
        int BreadthSpace = breadth < MaxSize.y ? breadth : (int)MaxSize.y;
        int WidthSpace = width;
        BreadthSpaceUsed = BreadthSpace;


        while (WidthSpace > MinSize.x)
        {
            int maxWidthSize = WidthSpace < MaxSize.x ? WidthSpace : (int)MaxSize.x;


            int decidedWidth = Random.Range((int)MinSize.x, maxWidthSize + 1);
            int decidedBreadth = Random.Range((int)MinSize.y, BreadthSpace+1);



            SizeShrinkModifier(district, out decidedWidth, out decidedBreadth, decidedWidth, decidedBreadth, MinSize,new Vector2(maxWidthSize,BreadthSpace));
         

            BuildingSizesLeft.Add(new Vector2(decidedWidth * perUnitMultiplier, decidedBreadth * perUnitMultiplier));

            WidthSpace -= decidedWidth;
        }

        return BuildingSizesLeft;

    }


    private List<Vector2> GenerateSideBuildingSizes(int width,int breadth,Vector2 MaxSize,Vector2 MinSize,out int WidthSpaceUsed,District district)
    {
        List<Vector2> BuildingSizesLeft = new List<Vector2>();
        int BreadthSpace = breadth;
        int WidthSpace = width < MaxSize.x ? width : (int)MaxSize.x;
        WidthSpaceUsed = WidthSpace;


        while (BreadthSpace > MinSize.y)
        {
            int maxBreadthSize = BreadthSpace < MaxSize.y ? BreadthSpace : (int)MaxSize.y;


            int decidedWidth = Random.Range((int)MinSize.x, WidthSpace + 1);
            int decidedBreadth = Random.Range((int)MinSize.y, maxBreadthSize + 1);




            SizeShrinkModifier(district, out decidedWidth, out decidedBreadth, decidedWidth, decidedBreadth, MinSize, new Vector2(WidthSpace, maxBreadthSize));


            BuildingSizesLeft.Add(new Vector2(decidedWidth * perUnitMultiplier, decidedBreadth * perUnitMultiplier));

            BreadthSpace -= decidedBreadth;
        }

        return BuildingSizesLeft;
    }

    //sizes are given in world units
    private void GenerateSideNeighborhood(Vector3 StartPosition, List<Vector2> BuildingSizesLeft,District district, int negation)
    {
        Vector3 Start = StartPosition;

        foreach(Vector2 BuildingSize in BuildingSizesLeft)
        {
            GameObject gameObject = new GameObject("BuildArea");
            

            Vector3 BuildingPosition = Start +
                BuildingSize.y/2.0f * -transform.forward +
                BuildingSize.x / 2.0f * transform.right * negation;


            gameObject.transform.position = BuildingPosition;
            gameObject.transform.LookAt(BuildingPosition - transform.right * negation, Vector3.up);

            BuildArea2D buildArea = gameObject.AddComponent<BuildArea2D>();
            buildArea.seed = Random.Range(0, 100);
            buildArea.BuildingStyle = DistrictToArchitectureStyle(district);

            buildArea.Width = (int)BuildingSize.y/perUnitMultiplier;
            buildArea.Breadth = (int)BuildingSize.x / perUnitMultiplier;

            Start += BuildingSize.y * -transform.forward;

            gameObject.transform.parent = transform;
        }


    }

    private void GenerateFrontalNeighborhood(Vector3 StartPosition, List<Vector2> BuildingSizesLeft, District district, int negation)
    {
        Vector3 Start = StartPosition;

        foreach (Vector2 BuildingSize in BuildingSizesLeft)
        {
            GameObject gameObject = new GameObject("FrontalSide");
           

            Vector3 BuildingPosition = Start +
                BuildingSize.x / 2.0f * transform.right +
                BuildingSize.y / 2.0f * -transform.forward * negation;


            gameObject.transform.position = BuildingPosition;
            gameObject.transform.LookAt(BuildingPosition + transform.forward * negation, Vector3.up);

            BuildArea2D buildArea = gameObject.AddComponent<BuildArea2D>();
            buildArea.seed = Random.Range(0, 100);
            buildArea.BuildingStyle = DistrictToArchitectureStyle(district);

            buildArea.Width = (int)BuildingSize.x / perUnitMultiplier;
            buildArea.Breadth = (int)BuildingSize.y / perUnitMultiplier;

            Start += BuildingSize.x * transform.right;

            gameObject.transform.parent = transform;
        }
    }

    private ArchitecturalStyle DistrictToArchitectureStyle(District district)
    {
        if(district == District.NeoMilitarism)
        {
            return ArchitecturalStyle.NeoMilitarism;
        }
        else if(district == District.Kitsch)
        {
            return ArchitecturalStyle.Kitsch;
        }
        else if(district == District.NeoKitsch)
        {
            return ArchitecturalStyle.NeoKitschMansion;
        }
        else if(district == District.Entropism)
        {
            return ArchitecturalStyle.Entropism;
        }
        else
        {
            return ArchitecturalStyle.Entropism;
        }

    }

    private void GetDistrictPrefferedSize(Neighborhood neighborhood,out Vector2 MinSize,out Vector2 MaxSize)
    {
        District district = neighborhood.district;

        switch(district)
        {
            case District.Kitsch:
                MinSize = BuildingUnitArray.KitschMinUnitSize;
                MaxSize = BuildingUnitArray.KitschMaxUnitSize; 

                break;
            case District.Entropism:
                MinSize = BuildingUnitArray.EntropismMinUnitSize;
                MaxSize = BuildingUnitArray.EntropismMaxUnitSize;
                break;

            case District.NeoMilitarism:
                MinSize = BuildingUnitArray.NeoMilitarismMinUnitSize;
                MaxSize = BuildingUnitArray.NeoMilitarismMaxUnitSize;
                break;

            case District.NeoKitsch:
                MinSize = BuildingUnitArray.NeoKitschMinUnitSize;
                MaxSize = BuildingUnitArray.NeoKitscMaxUnitSize;
                break;
            default:
                MinSize = Vector2.zero;
                MaxSize = Vector2.zero;
                break;
        }


    }

    private bool SizeShrinkModifier(District district,out int newWidth,out int newBreadth,int oldwidth,int oldBreadth,Vector2 minSize,Vector2 maxSize)
    {
        newWidth = oldwidth;
        newBreadth = oldBreadth;
        if (district == District.NeoKitsch)
        {
            bool isNeoMilitCopy = Random.Range(0, 1.0f) < NeoKitschMansionBuildingStrategy.NeoMilitCopyChance;

            if(isNeoMilitCopy)
            {
                int maxWidth = (int)NeoKitschMansionBuildingStrategy.PreferedNeoMilitCopySize.x < maxSize.x ? (int)NeoKitschMansionBuildingStrategy.PreferedNeoMilitCopySize.x : (int)maxSize.x;
                int maxBreadth = (int)NeoKitschMansionBuildingStrategy.PreferedNeoMilitCopySize.y < maxSize.y ? (int)NeoKitschMansionBuildingStrategy.PreferedNeoMilitCopySize.y : (int)maxSize.y;

                newWidth = Random.Range((int)minSize.x, maxWidth+1);
                newBreadth = Random.Range((int)minSize.y, maxBreadth+1);
                return true;
            }
        }
        return false;
        
    }

    void VoronoiSetDistrict(Neighborhood neighborhood, List<DistrictMarker>DistrictMarkers)
    {
        float closestDistance = float.MaxValue;
        District FinalDistrict = District.Entropism;

        foreach(DistrictMarker districtMarker in DistrictMarkers)
        {

            float currentDistance = Vector3.Distance(neighborhood.GetPosition(),districtMarker.transform.position);

            if( currentDistance < closestDistance)
            {
                closestDistance = currentDistance;
                FinalDistrict = districtMarker.GetDistrict();

            }



        }


        neighborhood.district = FinalDistrict;


    }

    public Vector3[] GetSquareVertices()
    {
        Vector3[] vertices = new Vector3[4];
        float SizedWidth = Width * perUnitMultiplier;
        float SizedBreadth = Breadth * perUnitMultiplier;


        Vector3 AddRight = SizedWidth / 2 * Vector3.Normalize(transform.right);
        Vector3 AddForward = SizedBreadth / 2 * Vector3.Normalize(transform.forward);


        vertices[0] = transform.position + AddRight + AddForward;
        vertices[1] = transform.position - AddRight + AddForward;
        vertices[2] = transform.position - AddRight - AddForward;
        vertices[3] = transform.position + AddRight - AddForward;




        return vertices;
    }

    public Vector3[] GetRelativeSquareVertices()
    {
        Vector3[] vertices = new Vector3[4];
        float SizedWidth = Width * perUnitMultiplier;
        float SizedBreadth = Breadth * perUnitMultiplier;


        Vector3 AddRight = SizedWidth / 2f * Vector3.Normalize(transform.right);
        Vector3 AddForward = SizedBreadth / 2f * Vector3.Normalize(transform.forward);


        vertices[0] = AddRight + AddForward;
        vertices[1] = -AddRight + AddForward;
        vertices[2] = -AddRight - AddForward;
        vertices[3] = AddRight - AddForward;




        return vertices;
    }

    private void OnDrawGizmos()
    {


        Vector3[] vertices = GetSquareVertices();

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(vertices[0], GizmoCircleRadius);
        Gizmos.DrawSphere(vertices[1], GizmoCircleRadius);
        Gizmos.DrawSphere(vertices[2], GizmoCircleRadius);
        Gizmos.DrawSphere(vertices[3], GizmoCircleRadius);

        Gizmos.color = Color.black;
        Gizmos.DrawLine(vertices[0], vertices[1]);
        Gizmos.DrawLine(vertices[1], vertices[2]);
        Gizmos.DrawLine(vertices[2], vertices[3]);
        Gizmos.DrawLine(vertices[3], vertices[0]);

    }


}

public enum LayoutGenerationOpt
{
    Undefined = 8,
    NoBuild,
    Road,
    VerticalDivide,
    HorizontalDivide,
    
}
