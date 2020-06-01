using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArchitecturalStylesArray : MonoBehaviour
{
    public int perUnitMultiplier;

    public GameObject[][] styles { get; private set; }
    // Start is called before the first frame update

    public GameObject[] NeoMilitarismChoices;
    public GameObject[] NeoKitschMansionChoices;
    public GameObject[] KitschChoices;
    public GameObject[] EntropismChoices;
    public GameObject[] NeoKitschSkyscraper;

    public int[] NeoMilitarismBuildingHeight = new int[2];
    public int[] NeoKitschMansionBuildingHeight= new int[2];
    public int[] KitschBuildingHeight = new int[2];
    public int[] EntropismBuildingHeight = new int[2];

    public Vector2 NeoMilitarismMinUnitSize = new Vector2(5, 5);
    public Vector2 NeoMilitarismMaxUnitSize = new Vector2(8, 8);

    public Vector2 NeoKitschMinUnitSize = new Vector2(7, 7);
    public Vector2 NeoKitscMaxUnitSize = new Vector2(11, 11);

    public Vector2 KitschMinUnitSize = new Vector2(5, 5);
    public Vector2 KitschMaxUnitSize = new Vector2(8, 8);

    public Vector2 EntropismMinUnitSize = new Vector2(4, 4);
    public Vector2 EntropismMaxUnitSize = new Vector2(8, 8);

    public GameObject roadTexture;
    public GameObject groundTexture;

    void Awake()
    {
        Init();
    }

    public void Init()
    {
        styles = new GameObject[System.Enum.GetNames(typeof(ArchitecturalStyle)).Length][];
        styles[(int)ArchitecturalStyle.NeoMilitarism] = new GameObject[NeoMilitarismChoices.Length];
        styles[(int)ArchitecturalStyle.NeoMilitarism] = NeoMilitarismChoices;

        styles[(int)ArchitecturalStyle.NeoKitschMansion] = new GameObject[NeoKitschMansionChoices.Length];
        styles[(int)ArchitecturalStyle.NeoKitschMansion] = NeoKitschMansionChoices;

        styles[(int)ArchitecturalStyle.Kitsch] = new GameObject[KitschChoices.Length];
        styles[(int)ArchitecturalStyle.Kitsch] = KitschChoices;

        styles[(int)ArchitecturalStyle.Entropism] = new GameObject[EntropismChoices.Length];
        styles[(int)ArchitecturalStyle.Entropism] = EntropismChoices;


        styles[(int)ArchitecturalStyle.NeoKitschSkyscraper] = new GameObject[NeoKitschSkyscraper.Length];
        styles[(int)ArchitecturalStyle.NeoKitschSkyscraper] = NeoKitschSkyscraper;
    }

    private void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
