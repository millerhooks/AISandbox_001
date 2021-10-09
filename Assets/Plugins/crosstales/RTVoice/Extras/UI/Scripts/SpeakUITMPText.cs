﻿using UnityEngine;
using UnityEngine.EventSystems;

namespace Crosstales.RTVoice.UI
{
   /// <summary>Speaks a TextMesh Pro text.</summary>
   [RequireComponent(typeof(TMPro.TextMeshPro))]
   [HelpURL("https://crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_u_i_1_1_speak_u_i_t_m_p_text.html")]
   public class SpeakUITMPText : SpeakUIBase
   {
#if false || CT_DEVELOP //Change this to "true" is you have TextMesh Pro installed

      #region Variables

      public bool ChangeColor = true;
      public Color TextColor = Color.green;
      public bool ClearTags = true;

      protected TMPro.TextMeshPro textComponent;
      private Color originalColor;

      #endregion


      #region MonoBehaviour methods

      private void Awake()
      {
         textComponent = GetComponent<TMPro.TextMeshPro>();
         originalColor = textComponent.color;
      }

      private void Update()
      {
         if (isInside)
         {
            elapsedTime += Time.deltaTime;

            if (elapsedTime > Delay && uid == null && (!SpeakOnlyOnce || !spoken))
            {
               if (ChangeColor)
                  textComponent.color = TextColor;

               uid = speak(ClearTags ? textComponent.text.CTClearTags() : textComponent.text);
               elapsedTime = 0f;
            }
         }
         else
         {
            elapsedTime = 0f;
         }
      }

      #endregion


      #region Overridden methods

      public override void OnPointerExit(PointerEventData eventData)
      {
         base.OnPointerExit(eventData);

         textComponent.color = originalColor;
      }

      protected override void onSpeakComplete(Model.Wrapper wrapper)
      {
         if (wrapper.Uid == uid)
         {
            base.onSpeakComplete(wrapper);

            textComponent.color = originalColor;
         }
      }

      #endregion

#else
      private void Awake()
      {
         Debug.LogWarning("Is TextMesh Pro installed? If so, please change line 9 of 'SpeakUITMPText.cs' to 'true'");
      }
#endif
   }
}
// © 2021 crosstales LLC (https://www.crosstales.com)