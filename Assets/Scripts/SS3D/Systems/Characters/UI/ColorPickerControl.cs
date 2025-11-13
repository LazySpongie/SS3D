
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SS3D.Core.Behaviours;
using System;
using SS3D.Systems.Characters.Preferences;

public class ColorPickerControl : Actor
{
    public delegate void ColorPickerValueChanged(ColorType type,Color color);

    public delegate void ColorPickerClosed();
    
    public event ColorPickerValueChanged OnValueChanged;

    public event ColorPickerClosed OnClosed;

    [SerializeField] private SVImageControl _svImageControl;

    [SerializeField] private RawImage _hueImage;
    [SerializeField] private RawImage _svImage;
    [SerializeField] private Image _outputImage;

    [SerializeField] private Slider _hueSlider;
    [SerializeField] private TMP_InputField _hexInputField;

    private Color _currentColor;
    
    public ColorType Type;

    public Color CurrentColor => _currentColor;

    public float _hue, _sat, _val;

    private Texture2D _hueTex;
    private Texture2D _svTex;

    protected override void OnAwake()
    {
        base.OnAwake();

        _hue = 1f;
        _sat = 1f;
        _val = 1f;
        CreateHueImage();
        CreateSVImage();
        SetOutputColor();
    }

    public void HandleCloseButtonPressed()
    {
        OnClosed?.Invoke();
    }

    public void SetColorFromHex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color color);

        SetColorFromRGB(color);
    }

    public void SetColorFromRGB(Color color)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);

        _hue = h;
        _sat = s;
        _val = v;
        UpdateColor();
    }

    public void SetSV(float s, float v)
    {
        _sat = s;
        _val = v;
        UpdateColor();
        OnValueChanged?.Invoke(Type, _currentColor);
    }

    private void UpdateColor()
    {
        SetOutputColor();
        UpdateSVImage();
        _hueSlider.SetValueWithoutNotify(_hue);
        _hexInputField.SetTextWithoutNotify(ColorUtility.ToHtmlStringRGB(_currentColor));
        _svImageControl.SetPointerPosition(_sat, _val);
    }

    private void SetOutputColor()
    {
        _currentColor = Color.HSVToRGB(_hue, _sat, _val);
        _outputImage.color = _currentColor;
    }

    public void HandleHueSliderChanged()
    {
        _hue = _hueSlider.value;
        UpdateColor();
        OnValueChanged?.Invoke(Type, _currentColor);
    }

    public void HandleHexInputChanged()
    {
        string text = _hexInputField.text;
        if (text.Length < 6) return;
        if (!ColorUtility.TryParseHtmlString("#" + text, out Color color)) return;
        SetColorFromRGB(color);
        OnValueChanged?.Invoke(Type, _currentColor);
    }

    private void CreateHueImage()
    {
        _hueTex = new Texture2D(1, 16);
        _hueTex.wrapMode = TextureWrapMode.Clamp;
        _hueTex.name = "HueTexture";

        for (int i = 0; i < _hueTex.height; i++)
        {
            _hueTex.SetPixel(0, i, Color.HSVToRGB((float)i / _hueTex.height, 1, 0.95f));
        }

        _hueTex.Apply();

        _hueImage.texture = _hueTex;
    }

    private void CreateSVImage()
    {
        _svTex = new Texture2D(16, 16);
        _svTex.wrapMode = TextureWrapMode.Clamp;
        _svTex.name = "SVTexture";

        UpdateSVImage();
        
        _svImage.texture = _svTex;
    }

    private void UpdateSVImage()
    {
        for (int y = 0; y < _svTex.height; y++)
        {
            for (int x = 0; x < _svTex.width; x++)
            {
                _svTex.SetPixel(x, y, Color.HSVToRGB(_hue, (float)x / _svTex.width, (float)y / _svTex.height));
            }
        }
        _svTex.Apply();
    }
}
