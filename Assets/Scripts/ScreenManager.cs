using UnityEngine;

/// <summary>
/// Manages the screens for a 360 video player.
/// </summary>
public class ScreenManager : MonoBehaviour {
    private const int FaceCount = 6;
    private const float ScreenWidth = 10f;
    private static readonly Vector3[] FaceEulerAngles = {
        new(90f, 0f, -90f), new(90f, 0f, 90f),
        new(0f, -180f, 0f), new(0f, 0f, -180f),
        new(90f, 0f, 0f), new(90f, -90f, 90f)
    };

    /// <summary>
    /// The number of tiles in each direction on a cubemap face.
    /// </summary>
    [field: SerializeField, Min(1)]
    public int TilingCount { get; set; } = 1;

    /// <summary>
    /// Whether to instantiate the screens in edit mode.
    /// </summary>
    [field: SerializeField]
    public bool InstantiateInEditMode { get; set; } = true;

    [field: SerializeField, HideInInspector] private GameObject screenPrefab;

    /// <summary>
    /// The renderers of the screens.
    /// </summary>
    public Renderer[] Renderers { get; private set; }

    /// <summary>
    /// Instantiates the screens.
    /// </summary>
    public void InstantiateScreens() {
        var screenCountPerFace = TilingCount * TilingCount;
        var screenCount = FaceCount * screenCountPerFace;
        var tileWidth = ScreenWidth / TilingCount;

        var positions = new Vector3[screenCount];
        var positionID = 0;
        foreach (var sign in new[] {-1, 1})
            for (var x = 0; x < TilingCount; ++x)
                for (var y = 0; y < TilingCount; ++y)
                    positions[positionID++] = new Vector3(sign * ScreenWidth / 2,
                        -(ScreenWidth - tileWidth) / 2 + y * tileWidth,
                        sign * ((ScreenWidth - tileWidth) / 2 - x * tileWidth));
        foreach (var sign in new[] {-1, 1})
            for (var x = 0; x < TilingCount; ++x)
                for (var y = 0; y < TilingCount; ++y)
                    positions[positionID++] = new Vector3(-(ScreenWidth - tileWidth) / 2 + x * tileWidth,
                        sign * ScreenWidth / 2,
                        sign * ((ScreenWidth - tileWidth) / 2 - y * tileWidth));
        foreach (var sign in new[] {-1, 1})
            for (var x = 0; x < TilingCount; ++x)
                for (var y = 0; y < TilingCount; ++y)
                    positions[positionID++] = new Vector3(-sign * ((ScreenWidth - tileWidth) / 2 - x * tileWidth),
                        -(ScreenWidth - tileWidth) / 2 + y * tileWidth,
                        sign * ScreenWidth / 2);

        Renderers = new Renderer[screenCount];
        for (var i = 0; i < screenCount; ++i) {
            var faceID = i / screenCountPerFace;
            var screen = Instantiate(screenPrefab, positions[i], Quaternion.Euler(FaceEulerAngles[faceID]), transform);
            screen.name = $"Screen{i}";
            screen.transform.localScale = Vector3.one / TilingCount;
            Renderers[i] = screen.GetComponent<Renderer>();
        }
    }
}
