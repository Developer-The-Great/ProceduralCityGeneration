using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(BuildArea2D))]
public class BuildingGenerator : MonoBehaviour
{
    private int seed;
    public int minLevel;
    public int maxLevel;
    public int CurrentBuildingLevel;

    private BuildArea2D BuildArea;
    [SerializeField]private GameObject[] MeshChoices;

    private ArchitecturalStylesArray BuildingUnitArray;
    private Shape2D InitShape;

    private void Awake()
    {
       
    }

    public void generate()
    {
        
        BuildingUnitArray = GameObject.FindGameObjectWithTag("Architecture").GetComponent<ArchitecturalStylesArray>();

        if(!BuildingUnitArray)
        {
            var x = Resources.FindObjectsOfTypeAll(typeof(ArchitecturalStylesArray));
            BuildingUnitArray = (ArchitecturalStylesArray)x[0];
            
        }




        BuildArea = GetComponent<BuildArea2D>();

        seed = BuildArea.seed;

        ArchitecturalStyle Style = BuildArea.BuildingStyle;

        BuildingUnitArray.Init();
        MeshChoices = BuildingUnitArray.styles[(int)Style];



        GameObject initialSeed = new GameObject("Shape2D");

        initialSeed.transform.position = gameObject.transform.position;
        initialSeed.transform.parent = gameObject.transform;

        InitShape = initialSeed.AddComponent<Shape2D>();


        switch (Style)
        {
            case ArchitecturalStyle.NeoMilitarism:
                minLevel = BuildingUnitArray.NeoMilitarismBuildingHeight[0];
                maxLevel = BuildingUnitArray.NeoMilitarismBuildingHeight[1];
                InitShape.Init(BuildArea.GetSquareVertices(), 1);

                NeoMilitaristBuildingStrategy.generateNeoMilitaristBuilding(
            MeshChoices, transform, BuildArea,
            InitShape, seed, minLevel, maxLevel,
            ref CurrentBuildingLevel, false, true);
                break;

            case ArchitecturalStyle.NeoKitschMansion:
                minLevel = BuildingUnitArray.NeoKitschMansionBuildingHeight[0];
                maxLevel = BuildingUnitArray.NeoKitschMansionBuildingHeight[1];

                NeoKitschMansionBuildingStrategy.generateNeoKitschMansionBuilding(MeshChoices, transform, BuildArea,
            InitShape, seed, minLevel, maxLevel,
            ref CurrentBuildingLevel, BuildingUnitArray.styles[(int)ArchitecturalStyle.NeoKitschSkyscraper]);
                break;


            case ArchitecturalStyle.Kitsch:
                minLevel = BuildingUnitArray.KitschBuildingHeight[0];
                maxLevel = BuildingUnitArray.KitschBuildingHeight[1];

                KitschBuildingStrategy.generateKitschBuilding(MeshChoices, transform, BuildArea,
            InitShape, seed, minLevel, maxLevel,
            ref CurrentBuildingLevel);

                break;

            case ArchitecturalStyle.Entropism:
                minLevel = BuildingUnitArray.EntropismBuildingHeight[0];
                maxLevel = BuildingUnitArray.EntropismBuildingHeight[1];

                EntropismBuildingStrategy.generateEntropismBuilding(MeshChoices, transform, BuildArea,
            InitShape, seed, minLevel, maxLevel,
            ref CurrentBuildingLevel);
                break;


        }
    }
    // Start is called before the first frame update
    void Start()
    {
        //generate();




    }

    

}

