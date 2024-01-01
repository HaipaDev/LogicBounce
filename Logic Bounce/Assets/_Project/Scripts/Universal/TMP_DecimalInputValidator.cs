using UnityEngine;
using System;
using System.Globalization;
using UnityEngine.UI;

namespace TMPro{
    [Serializable]
    [CreateAssetMenu(menuName = "TextMeshPro/Input Validators/Decimal")]
    public class TMP_DecimalInputValidator : TMP_InputValidator{
        [SerializeField] float minValue = 0f;
        [SerializeField] float maxValue = 100f;
        bool _debug=true;
        public override char Validate(ref string text, ref int pos, char ch){
            if(_debug)Debug.Log("PRE ;; text: "+text+" | pos: "+pos+" | ch: "+ch);
            Clamp(ref text);
            if(_debug)Debug.Log("Clamped text: "+text);
            float floatValue;
            float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out floatValue);
            if(_debug)Debug.Log("Float value: "+floatValue);

            if(floatValue>maxValue){
                text=maxValue.ToString();
                return (char)0;
            }else if(floatValue<minValue){
                text=minValue.ToString();
                return (char)0;
            }

            if(ch >= '0' && ch <= '9'){
                if(text == "0" && !text.Contains(".") && !text.Contains(",")){
                    text = ch.ToString(); // Replace the leading zero with the input digit
                }else{
                    text = text.Insert(pos, ch.ToString());
                    pos += 1;
                }
                return ParseClampAfter(ref text,ref floatValue,pos,ch);
            }else if((ch == '.' || ch == ',') && !text.Contains(".") && !text.Contains(",") && floatValue<maxValue){
                if(pos == 0){
                    text = "0" + ch; // Convert leading decimal separator to "0."
                    pos = 2; // Set the position to after the "0."
                }else{
                    text += '.'; // Insert the decimal separator
                    pos += 1;
                }
                return ParseClampAfter(ref text,ref floatValue,pos,ch);
            }else if(ch == '-' && !text.Contains("-")){
                if(pos == 0){
                    text = "-" + text; // Add the negative sign at the beginning
                    pos += 1;
                }
                return ParseClampAfter(ref text,ref floatValue,pos,ch);
            }

            return (char)0; // Disallow any other characters

            char ParseClampAfter(ref string text,ref float floatValue, int pos, char ch){
                Clamp(ref text);
                float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out floatValue);
                if(floatValue>maxValue){
                    text=maxValue.ToString();
                    return (char)0;
                }else if(floatValue<minValue){
                    text=minValue.ToString();
                    return (char)0;
                }
                if(_debug)Debug.Log("POST ;; text: "+text+" | pos: "+pos+" | ch: "+ch);
                if(_debug)Debug.Log("Float value: "+floatValue);
                return ch;
            }
        }


        void Clamp(ref string text){
            if(float.TryParse(text, out float value)){
                if(_debug)Debug.Log("PreClamped parse: "+value);
                value = Mathf.Clamp(value, minValue, maxValue);

                // Round down to the nearest integer if the value exceeds the maxValue
                if(value>maxValue&&Mathf.Floor(value)==maxValue){value=Mathf.Floor(value);text=value.ToString("F0");}
                else{text=value.ToString();}

                // Format the value as a string with the appropriate decimal places
                //int decimalPlaces = Mathf.Max(0, Mathf.FloorToInt(Mathf.Log10(maxValue) + 1));
                //float roundedValue = Mathf.Round(value * Mathf.Pow(10, decimalPlaces)) / Mathf.Pow(10, decimalPlaces);

                // Format the rounded value as a string
                //text = roundedValue.ToString("F" + decimalPlaces.ToString());

                /*if(Mathf.Approximately(value, Mathf.Round(value))){
                    text = value.ToString("F0");
                }else{
                    text = value.ToString();
                }*/
                /*if(float.TryParse(text, out float value)){
                    value = Mathf.Clamp(value, minValue, maxValue);
                    text = value.ToString();
                }*/
            }else{if(_debug){Debug.LogWarning("Cant parse "+text+" to float");}}
        }
    }
}