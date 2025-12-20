using UnityEngine;

/// <summary>
/// 寿司カウンターを上から照らすスポットライトを生成するスクリプト
/// シーン開始時に自動的にライトを作成します。
/// </summary>
public class StageLighting : MonoBehaviour
{
    [Header("Lighting Settings")]
    public float intensity = 2.0f;
    public Color lightColor = new Color(1.0f, 0.95f, 0.8f); // 暖かみのある白
    public float range = 20.0f;
    public float spotAngle = 100.0f;

    void Start()
    {
        // アタッチされているオブジェクトのLightコンポーネントを取得
        Light light = GetComponent<Light>();

        // もしLightコンポーネントがなければ追加する
        if (light == null)
        {
            light = gameObject.AddComponent<Light>();
        }

        SetupLight(light);
    }

    void SetupLight(Light light)
    {
        // 位置と回転の強制設定を削除（ユーザーが自由に動かせるように）
        // transform.position = new Vector3(0, 3.0f, 0.5f);
        // transform.rotation = Quaternion.Euler(90, 0, 0);

        // Light設定を適用
        light.type = LightType.Spot;
        light.intensity = intensity;
        light.color = lightColor;
        light.range = range;
        light.spotAngle = spotAngle;
        
        // 影を有効化
        light.shadows = LightShadows.Soft;

        // ボリューメトリック風のパーティクル生成
        CreateFogEffect(light.gameObject, range, spotAngle);

        Debug.Log("ステージ照明を設定しました！");
    }

    void CreateFogEffect(GameObject parent, float range, float angle)
    {
        // 既にエフェクトがあるか確認し、あれば取得、なければ作成
        Transform existingFog = parent.transform.Find("LightFog");
        GameObject fogObj;
        ParticleSystem ps;

        if (existingFog != null)
        {
            fogObj = existingFog.gameObject;
            ps = fogObj.GetComponent<ParticleSystem>();
            if (ps == null) ps = fogObj.AddComponent<ParticleSystem>();
        }
        else
        {
            fogObj = new GameObject("LightFog");
            fogObj.transform.SetParent(parent.transform, false);
            fogObj.transform.localPosition = Vector3.zero;
            fogObj.transform.localRotation = Quaternion.identity;
            ps = fogObj.AddComponent<ParticleSystem>();
        }

        ParticleSystemRenderer psr = ps.GetComponent<ParticleSystemRenderer>();

        // --- 設定を強制的に上書き ---
        
        // --- Main Module ---
        var main = ps.main;
        main.startLifetime = 10.0f;      // 寿命をとても長くする
        main.startSpeed = 0.5f;          // 速度をかなり落として「漂う」感じにする（流れ落ちない）
        main.startSize = 0.4f;           
        main.startColor = new Color(1f, 0.95f, 0.8f, 0.04f); // アルファを下げて「うっすら」に戻す
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 1000;
        main.prewarm = true;             // 最初から部屋中に漂っている状態にする

        // --- Emission ---
        var emission = ps.emission;
        emission.rateOverTime = 50;      // 量を減らして「モクモク感」を消す

        // --- Shape ---
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25.0f;
        shape.radius = 0.1f;
        shape.length = range;            
        shape.position = Vector3.zero;
        shape.rotation = Vector3.zero;

        // --- Size over Lifetime ---
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 0.4f);
        curve.AddKey(1.0f, 0.6f);        // サイズ変化を控えめにする
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, curve);

        // --- Color over Lifetime ---
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.0f), new GradientAlphaKey(1.0f, 0.1f), new GradientAlphaKey(1.0f, 0.9f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = grad;

        // --- Rotation over Lifetime ---
        var rotationOverLifetime = ps.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(1.0f * Mathf.Deg2Rad); // ほぼ回転させない（動きを目立たせない）

        // --- Renderer ---
        psr.material = new Material(Shader.Find("Sprites/Default"));
        psr.renderMode = ParticleSystemRenderMode.Billboard;
    }
}
