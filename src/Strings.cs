using System.Collections.Generic;
using UnityEngine;

namespace RestoryReColor;

internal static class Strings
{
    private static readonly Dictionary<string, Dictionary<SystemLanguage, string>> Translations = new()
    {
        [nameof(SurfaceStyle)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "Surface style" },
            { SystemLanguage.Russian, "Поверхность" },
            { SystemLanguage.Portuguese, "Superfície" },
            { SystemLanguage.Spanish, "Superficie" },
            { SystemLanguage.French, "Surface" },
            { SystemLanguage.German, "Oberfläche" },
            { SystemLanguage.Japanese, "表面" },
            { SystemLanguage.Korean, "표면" },
            { SystemLanguage.ChineseSimplified, "表面" },
            { SystemLanguage.Chinese, "表面" }
        },
        [nameof(BaseTexture)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "Base texture" },
            { SystemLanguage.Russian, "Текстура" },
            { SystemLanguage.Portuguese, "Textura" },
            { SystemLanguage.Spanish, "Textura" },
            { SystemLanguage.French, "Texture" },
            { SystemLanguage.German, "Textur" },
            { SystemLanguage.Japanese, "テクスチャ" },
            { SystemLanguage.Korean, "텍스처" },
            { SystemLanguage.ChineseSimplified, "贴图" },
            { SystemLanguage.Chinese, "貼圖" }
        },
        [nameof(Colour)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "Colour" },
            { SystemLanguage.Russian, "Цвет" },
            { SystemLanguage.Portuguese, "Cor" },
            { SystemLanguage.Spanish, "Color" },
            { SystemLanguage.French, "Couleur" },
            { SystemLanguage.German, "Farbe" },
            { SystemLanguage.Japanese, "色" },
            { SystemLanguage.Korean, "색상" },
            { SystemLanguage.ChineseSimplified, "颜色" },
            { SystemLanguage.Chinese, "顏色" }
        },
        [nameof(Normal)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "Normal" },
            { SystemLanguage.Russian, "Обычный" },
            { SystemLanguage.Portuguese, "Normal" },
            { SystemLanguage.Spanish, "Normal" },
            { SystemLanguage.French, "Normal" },
            { SystemLanguage.German, "Normal" },
            { SystemLanguage.Japanese, "通常" },
            { SystemLanguage.Korean, "기본" },
            { SystemLanguage.ChineseSimplified, "普通" },
            { SystemLanguage.Chinese, "普通" }
        },
        [nameof(Tournament)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "Tournament" },
            { SystemLanguage.Russian, "Турнир" },
            { SystemLanguage.Portuguese, "Torneio" },
            { SystemLanguage.Spanish, "Torneo" },
            { SystemLanguage.French, "Tournoi" },
            { SystemLanguage.German, "Turnier" },
            { SystemLanguage.Japanese, "大会" },
            { SystemLanguage.Korean, "대회" },
            { SystemLanguage.ChineseSimplified, "比赛" },
            { SystemLanguage.Chinese, "比賽" }
        },
        [nameof(Table)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "Table" },
            { SystemLanguage.Russian, "Стол" },
            { SystemLanguage.Portuguese, "Mesa" },
            { SystemLanguage.Spanish, "Mesa" },
            { SystemLanguage.French, "Table" },
            { SystemLanguage.German, "Tisch" },
            { SystemLanguage.Japanese, "机" },
            { SystemLanguage.Korean, "책상" },
            { SystemLanguage.ChineseSimplified, "桌子" },
            { SystemLanguage.Chinese, "桌子" }
        },
        [nameof(Original)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "Original" },
            { SystemLanguage.Russian, "Оригинал" },
            { SystemLanguage.Portuguese, "Original" },
            { SystemLanguage.Spanish, "Original" },
            { SystemLanguage.French, "Original" },
            { SystemLanguage.German, "Original" },
            { SystemLanguage.Japanese, "オリジナル" },
            { SystemLanguage.Korean, "원본" },
            { SystemLanguage.ChineseSimplified, "原始" },
            { SystemLanguage.Chinese, "原始" }
        },
        [nameof(Greyscale)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "Greyscale" },
            { SystemLanguage.Russian, "Оттенки серого" },
            { SystemLanguage.Portuguese, "Escala de cinza" },
            { SystemLanguage.Spanish, "Escala de grises" },
            { SystemLanguage.French, "Niveaux de gris" },
            { SystemLanguage.German, "Graustufen" },
            { SystemLanguage.Japanese, "グレースケール" },
            { SystemLanguage.Korean, "회색조" },
            { SystemLanguage.ChineseSimplified, "灰度" },
            { SystemLanguage.Chinese, "灰階" }
        },
        [nameof(Flat)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "Flat" },
            { SystemLanguage.Russian, "Заливка" },
            { SystemLanguage.Portuguese, "Sólido" },
            { SystemLanguage.Spanish, "Sólido" },
            { SystemLanguage.French, "Uni" },
            { SystemLanguage.German, "Einfarbig" },
            { SystemLanguage.Japanese, "単色" },
            { SystemLanguage.Korean, "단색" },
            { SystemLanguage.ChineseSimplified, "纯色" },
            { SystemLanguage.Chinese, "純色" }
        },
        [nameof(Red)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "Red" },
            { SystemLanguage.Russian, "Красный" },
            { SystemLanguage.Portuguese, "Vermelho" },
            { SystemLanguage.Spanish, "Rojo" },
            { SystemLanguage.French, "Rouge" },
            { SystemLanguage.German, "Rot" },
            { SystemLanguage.Japanese, "赤" },
            { SystemLanguage.Korean, "빨강" },
            { SystemLanguage.ChineseSimplified, "红色" },
            { SystemLanguage.Chinese, "紅色" }
        },
        [nameof(Green)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "Green" },
            { SystemLanguage.Russian, "Зелёный" },
            { SystemLanguage.Portuguese, "Verde" },
            { SystemLanguage.Spanish, "Verde" },
            { SystemLanguage.French, "Vert" },
            { SystemLanguage.German, "Grün" },
            { SystemLanguage.Japanese, "緑" },
            { SystemLanguage.Korean, "초록" },
            { SystemLanguage.ChineseSimplified, "绿色" },
            { SystemLanguage.Chinese, "綠色" }
        },
        [nameof(Blue)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "Blue" },
            { SystemLanguage.Russian, "Синий" },
            { SystemLanguage.Portuguese, "Azul" },
            { SystemLanguage.Spanish, "Azul" },
            { SystemLanguage.French, "Bleu" },
            { SystemLanguage.German, "Blau" },
            { SystemLanguage.Japanese, "青" },
            { SystemLanguage.Korean, "파랑" },
            { SystemLanguage.ChineseSimplified, "蓝色" },
            { SystemLanguage.Chinese, "藍色" }
        },
        [nameof(BasePaint)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "Base paint" },
            { SystemLanguage.Russian, "Основной цвет" },
            { SystemLanguage.Portuguese, "Cor base" },
            { SystemLanguage.Spanish, "Color base" },
            { SystemLanguage.French, "Couleur de base" },
            { SystemLanguage.German, "Grundfarbe" },
            { SystemLanguage.Japanese, "ベースカラー" },
            { SystemLanguage.Korean, "기본 색상" },
            { SystemLanguage.ChineseSimplified, "基础颜色" },
            { SystemLanguage.Chinese, "基礎顏色" }
        },
        [nameof(Stripes)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "Stripes" },
            { SystemLanguage.Russian, "Полосы" },
            { SystemLanguage.Portuguese, "Listras" },
            { SystemLanguage.Spanish, "Rayas" },
            { SystemLanguage.French, "Rayures" },
            { SystemLanguage.German, "Streifen" },
            { SystemLanguage.Japanese, "ストライプ" },
            { SystemLanguage.Korean, "줄무늬" },
            { SystemLanguage.ChineseSimplified, "条纹" },
            { SystemLanguage.Chinese, "條紋" }
        },
        [nameof(StripesAdditive)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "Stripes additive" },
            { SystemLanguage.Russian, "Свечение полос" },
            { SystemLanguage.Portuguese, "Brilho das listras" },
            { SystemLanguage.Spanish, "Brillo de rayas" },
            { SystemLanguage.French, "Éclat des rayures" },
            { SystemLanguage.German, "Streifenleuchten" },
            { SystemLanguage.Japanese, "ストライプ発光" },
            { SystemLanguage.Korean, "줄무늬 발광" },
            { SystemLanguage.ChineseSimplified, "条纹发光" },
            { SystemLanguage.Chinese, "條紋發光" }
        },
        [nameof(Shine)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "Shine" },
            { SystemLanguage.Russian, "Блеск" },
            { SystemLanguage.Portuguese, "Brilho" },
            { SystemLanguage.Spanish, "Brillo" },
            { SystemLanguage.French, "Brillance" },
            { SystemLanguage.German, "Glanz" },
            { SystemLanguage.Japanese, "光沢" },
            { SystemLanguage.Korean, "광택" },
            { SystemLanguage.ChineseSimplified, "光泽" },
            { SystemLanguage.Chinese, "光澤" }
        },
        [nameof(ShineIntensity)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "Shine intensity" },
            { SystemLanguage.Russian, "Сила блеска" },
            { SystemLanguage.Portuguese, "Intensidade do brilho" },
            { SystemLanguage.Spanish, "Intensidad del brillo" },
            { SystemLanguage.French, "Intensité de brillance" },
            { SystemLanguage.German, "Glanzstärke" },
            { SystemLanguage.Japanese, "光沢の強さ" },
            { SystemLanguage.Korean, "광택 강도" },
            { SystemLanguage.ChineseSimplified, "光泽强度" },
            { SystemLanguage.Chinese, "光澤強度" }
        },
        [nameof(EmissionPower)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "Emission power" },
            { SystemLanguage.Russian, "Свечение" },
            { SystemLanguage.Portuguese, "Emissão" },
            { SystemLanguage.Spanish, "Emisión" },
            { SystemLanguage.French, "Émission" },
            { SystemLanguage.German, "Leuchtkraft" },
            { SystemLanguage.Japanese, "発光の強さ" },
            { SystemLanguage.Korean, "발광 세기" },
            { SystemLanguage.ChineseSimplified, "自发光强度" },
            { SystemLanguage.Chinese, "自發光強度" }
        },
        [nameof(Specular)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "Specular" },
            { SystemLanguage.Russian, "Отражение" },
            { SystemLanguage.Portuguese, "Reflexo" },
            { SystemLanguage.Spanish, "Reflejo" },
            { SystemLanguage.French, "Réflexion" },
            { SystemLanguage.German, "Reflexion" },
            { SystemLanguage.Japanese, "反射" },
            { SystemLanguage.Korean, "반사" },
            { SystemLanguage.ChineseSimplified, "高光" },
            { SystemLanguage.Chinese, "高光" }
        },
        [nameof(Metallic)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "Metallic" },
            { SystemLanguage.Russian, "Металлик" },
            { SystemLanguage.Portuguese, "Metálico" },
            { SystemLanguage.Spanish, "Metálico" },
            { SystemLanguage.French, "Métallique" },
            { SystemLanguage.German, "Metallisch" },
            { SystemLanguage.Japanese, "メタリック" },
            { SystemLanguage.Korean, "메탈릭" },
            { SystemLanguage.ChineseSimplified, "金属感" },
            { SystemLanguage.Chinese, "金屬感" }
        },
        [nameof(Brightness)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "Brightness" },
            { SystemLanguage.Russian, "Яркость" },
            { SystemLanguage.Portuguese, "Brilho" },
            { SystemLanguage.Spanish, "Brillo" },
            { SystemLanguage.French, "Luminosité" },
            { SystemLanguage.German, "Helligkeit" },
            { SystemLanguage.Japanese, "明るさ" },
            { SystemLanguage.Korean, "밝기" },
            { SystemLanguage.ChineseSimplified, "亮度" },
            { SystemLanguage.Chinese, "亮度" }
        },
        [nameof(Contrast)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "Contrast" },
            { SystemLanguage.Russian, "Контраст" },
            { SystemLanguage.Portuguese, "Contraste" },
            { SystemLanguage.Spanish, "Contraste" },
            { SystemLanguage.French, "Contraste" },
            { SystemLanguage.German, "Kontrast" },
            { SystemLanguage.Japanese, "コントラスト" },
            { SystemLanguage.Korean, "대비" },
            { SystemLanguage.ChineseSimplified, "对比度" },
            { SystemLanguage.Chinese, "對比度" }
        },
        [nameof(PaintTab)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "PAINT" },
            { SystemLanguage.Russian, "ЦВЕТ" },
            { SystemLanguage.Portuguese, "PINTAR" },
            { SystemLanguage.Spanish, "PINTAR" },
            { SystemLanguage.French, "PEINDRE" },
            { SystemLanguage.German, "MALEN" },
            { SystemLanguage.Japanese, "ぬる" },
            { SystemLanguage.Korean, "칠하기" },
            { SystemLanguage.ChineseSimplified, "上色" },
            { SystemLanguage.Chinese, "上色" }
        },
        [nameof(PresetTab)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "PRESET" },
            { SystemLanguage.Russian, "СТИЛЬ" },
            { SystemLanguage.Portuguese, "ESTILO" },
            { SystemLanguage.Spanish, "ESTILO" },
            { SystemLanguage.French, "STYLE" },
            { SystemLanguage.German, "STIL" },
            { SystemLanguage.Japanese, "プリセット" },
            { SystemLanguage.Korean, "프리셋" },
            { SystemLanguage.ChineseSimplified, "预设" },
            { SystemLanguage.Chinese, "預設" }
        },
        [nameof(ResetTab)] = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "RESET" },
            { SystemLanguage.Russian, "СБРОС" },
            { SystemLanguage.Portuguese, "REPOR" },
            { SystemLanguage.Spanish, "REINICIAR" },
            { SystemLanguage.French, "RESET" },
            { SystemLanguage.German, "RESET" },
            { SystemLanguage.Japanese, "リセット" },
            { SystemLanguage.Korean, "초기화" },
            { SystemLanguage.ChineseSimplified, "重置" },
            { SystemLanguage.Chinese, "重置" }
        }
    };

    private static SystemLanguage _language = SystemLanguage.English;

    internal static string SurfaceStyle => Get(nameof(SurfaceStyle));

    internal static string BaseTexture => Get(nameof(BaseTexture));

    internal static string Colour => Get(nameof(Colour));

    internal static string Normal => Get(nameof(Normal));

    internal static string Tournament => Get(nameof(Tournament));

    internal static string Table => Get(nameof(Table));

    internal static string Original => Get(nameof(Original));

    internal static string Greyscale => Get(nameof(Greyscale));

    internal static string Flat => Get(nameof(Flat));

    internal static string Red => Get(nameof(Red));

    internal static string Green => Get(nameof(Green));

    internal static string Blue => Get(nameof(Blue));

    internal static string BasePaint => Get(nameof(BasePaint));

    internal static string Stripes => Get(nameof(Stripes));

    internal static string StripesAdditive => Get(nameof(StripesAdditive));

    internal static string Shine => Get(nameof(Shine));

    internal static string ShineIntensity => Get(nameof(ShineIntensity));

    internal static string EmissionPower => Get(nameof(EmissionPower));

    internal static string Specular => Get(nameof(Specular));

    internal static string Metallic => Get(nameof(Metallic));

    internal static string Brightness => Get(nameof(Brightness));

    internal static string Contrast => Get(nameof(Contrast));

    internal static string PaintTab => Get(nameof(PaintTab));

    internal static string PresetTab => Get(nameof(PresetTab));

    internal static string ResetTab => Get(nameof(ResetTab));

    internal static void SetLanguage(SystemLanguage language)
    {
        _language = language;
    }

    private static string Get(string key)
    {
        Dictionary<SystemLanguage, string> byLanguage = Translations[key];

        if (byLanguage.TryGetValue(_language, out string translated))
            return translated;

        return byLanguage[SystemLanguage.English];
    }
}
