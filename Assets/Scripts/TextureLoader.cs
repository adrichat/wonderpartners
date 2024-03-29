using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class AdvancedTextureLoader : MonoBehaviour
{
    public string baseMapUrl;
    public string emissiveMapUrl;
    public string occlusionMapUrl;
    public string metallicRoughnessMapUrl;
    public string normalMapUrl;

    private Material material;

    void Start()
    {
        material = GetComponent<Renderer>().material;
        StartCoroutine(LoadTexture(baseMapUrl, texture => material.mainTexture = texture));

        StartCoroutine(LoadTexture(emissiveMapUrl, texture =>
        {
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            material.SetColor("_EmissionColor", Color.white);
            material.SetTexture("_EmissionMap", texture);
        }));

        StartCoroutine(LoadTexture(occlusionMapUrl, texture => material.SetTexture("_OcclusionMap", texture)));

        StartCoroutine(LoadAndConvertMetallicRoughness(metallicRoughnessMapUrl));

        StartCoroutine(LoadTexture(normalMapUrl, texture => ApplyNormalMap(texture)));
    }

    IEnumerator LoadTexture(string url, System.Action<Texture2D> applyTexture)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(uwr.error);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                applyTexture(texture);
            }
        }
    }

    void ApplyNormalMap(Texture2D normalMap)
    {
        Texture2D normalMapTexture = new Texture2D(normalMap.width, normalMap.height, TextureFormat.RGBA32, true);
        Graphics.CopyTexture(normalMap, normalMapTexture);


        for (int i = 0; i < normalMapTexture.width; i++)
        {
            for (int j = 0; j < normalMapTexture.height; j++)
            {
                Color pixel = normalMapTexture.GetPixel(i, j);
                pixel.a = pixel.r;
                pixel.r = 0.5f;
                pixel.g = 0.5f;
                normalMapTexture.SetPixel(i, j, pixel);
            }
        }
        normalMapTexture.Apply();

        material.SetTexture("_BumpMap", normalMapTexture);
        material.SetFloat("_BumpScale", 1.0f);
    }

    IEnumerator LoadAndConvertMetallicRoughness(string url)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(uwr.error);
            }
            else
            {
                // Charger la texture originale
                Texture2D originalTexture = DownloadHandlerTexture.GetContent(uwr);

                // Créer une nouvelle texture avec les canaux ajustés
                Texture2D convertedTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);

                // Remplir la nouvelle texture
                for (int x = 0; x < originalTexture.width; x++)
                {
                    for (int y = 0; y < originalTexture.height; y++)
                    {
                        Color originalColor = originalTexture.GetPixel(x, y);
                        // La métallicité de glTF est dans le canal bleu, et la rugosité est dans le canal vert.
                        // Unity s'attend à ce que la métallicité soit dans le canal rouge et la rugosité dans le canal alpha.
                        Color convertedColor = new Color(originalColor.b, 0, 0, 1 - originalColor.g); // Inverser la rugosité
                        convertedTexture.SetPixel(x, y, convertedColor);
                    }
                }

                convertedTexture.Apply();

                // Appliquer la texture convertie
                material.SetTexture("_MetallicGlossMap", convertedTexture);
                material.SetFloat("_Metallic", 1.0f); 
                material.SetFloat("_Glossiness", 1.0f); 
            }
        }
    }
}
