
using System;
using System.Collections;
using Emotion;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Female
{
    public class FemalePlayer : Character
    {
        FemalePlayer()
        {
            EnergyCharacter = new PlayerEnergy(this);
            Name = "Kira";
        }

        internal override void Start()
        {
            base.Start();
            SceneManager.sceneLoaded += UpdateEmotionSet;
            StartCoroutine(RunAI());
            //Добавить слушателя события Актион.Евент чтобы записывать их в АктионКонтролПлейер для сбора статистики дейтсвий игрока
            Actions.EventActionsControlPlayer += ActionsControlPlayer.AddActionsControlPlayer;
            Lua.RegisterFunction("CustomGET_CountInteractionPlayer", null, SymbolExtensions.GetMethodInfo(() =>
                ActionsControlPlayer.DoubleGetStaticInteractionPLayer(String.Empty, String.Empty)));
            Lua.RegisterFunction("CustomGET_BoolInteractionPlayer", null, SymbolExtensions.GetMethodInfo(() =>
                ActionsControlPlayer.BoolGetStaticInteractionPLayer(String.Empty, String.Empty)));

            if ((Work = Work.Jobless) != Work.Cafe)
            {
                DialogueManager.ShowAlert("Я должна найти работу"); //Локализовать
            }
        }

        private void UpdateEmotionSet(Scene arg0, LoadSceneMode arg1)
        {
            EmotionSet();
        }

        public override GameObject GameObject()
        {
            return this.gameObject;
        }

        protected override void EmotionSet()
        {
            DialogCharacter = GameObject().GetComponent<PlayerDialog>();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= UpdateEmotionSet;
            Actions.EventActionsControlPlayer -= ActionsControlPlayer.AddActionsControlPlayer;
            Lua.UnregisterFunction("CustomGET_CountInteractionPlayer");
            Lua.UnregisterFunction("CustomGET_BoolInteractionPlayer");
        }
        
        private IEnumerator RunAI()
        {
            yield return new WaitForSeconds(0.1f);
            if (gameObject.GetComponent<AI>() != null) Ai = gameObject.GetComponent<AI>();
            Ai.Switch_Idle(new AIIdlePlayerDefault());
            Ai.SwitchActivity(ActivityAI.Idle, _floatWaitUpdateAI: 40f);
            Ai.AI_enabled = false;
        }
    }
        
}
