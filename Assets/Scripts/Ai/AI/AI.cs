
using System;
using System.Collections;
using System.Collections.Generic;
using ActionsNPC;
using Ai.AI.Waiting;
using RootMotion.FinalIK;
using Temperament;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class AI : MonoBehaviour
{
    /// <summary>
    /// Текущая активность АИ
    /// </summary>
    private ActivityAI _activityAI;

    private string _localizationID = "Idle";
    internal string LocalizationID
    {
        get => _localizationID;
        private set => _localizationID = value;
    }

    protected Character _myCharacter;
    protected GameObject GameObjectPlayer;
    /// <summary>
    /// Можно ли АИ самостоятельно работать
    /// </summary>
    internal bool AI_enabled = true;
    /// <summary>
    /// Текущая активность НПС
    /// </summary>
    internal ActionsNPC.ActionsNPC CurrentActions; //текущая активность НПС
    /// <summary>
    /// Лист активностей (для функции свич активити. чтобы не плодить списков)
    /// </summary>
    private List<ActionsNPC.ActionsNPC> privateListActionsNPC = new List<ActionsNPC.ActionsNPC>();
    
    [SerializeField] protected GameCreator.Characters.Character _characterNPC;

    protected IWorkActivity _workActivity = new AiDefaultWork(); //заглушка для работы
    protected IIdle _idle = new AiDefaultIdle();
    protected IFollow _follow = new AiDefaultFollow();
    protected ITalking _talking = new AiDefaultTalking(); //заглушка для диалога. Отображает сообщение над головой
    protected IWaiting _waiting = new AiDefaultWaiting();

    protected float _controlFloatWaitUpdateAI;
    protected string _controlstringID_message;

    protected Actions[] _actionses;
    
    private void Start()
    {
        _actionses = GetComponentsInChildren<Actions>();
        _myCharacter = _characterNPC.gameObject.GetComponent<Character>();
        if (_characterNPC.characterLocomotion.canUseNavigationMesh)
        {
            _characterNPC.characterLocomotion.navmeshAgent.baseOffset = 0.065f;
            _characterNPC.characterLocomotion.navmeshAgent.speed = 1f;
        }
        
        StartCoroutine(StateUpdate());
        GameObjectPlayer = GameObject.FindWithTag("Player");
  
    }

    /// <summary>
    /// Отключить автообновление АИ
    /// </summary>
    internal void DisableAutoStateUpdate()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// Включить автообновление АИ
    /// </summary>
    internal void StartAutoStateUpdate()
    {
        StartCoroutine(StateUpdate());
    }

    /// <summary>
    /// Запуск активностиНПС принудительно (АИ отключится)
    /// </summary>
    /// <param name="actionsNpc"></param>
    internal void RunActionNPC(ActionsNPC.ActionsNPC actionsNpc)
    {
        _activityAI = actionsNpc.ActivityAi();
        AI_enabled = false;
        if (CurrentActions != null)
        {
            CurrentActions.StopAction();
        }
        CurrentActions = actionsNpc;
        CurrentActions.UpdateNameCharacterAndStartFollow(_myCharacter);
    }
    
    
    /// <summary>
    /// переключить активность
    /// </summary>
    /// <param name="activityAi"></param>
    /// <param name="transformTarget1"></param>
    /// <param name="transformTarget2"></param>
    public void SwitchActivity(ActivityAI activityAi, GameObject transformTarget1 = null, GameObject transformTarget2 = null, string ID_dialogMessage = "HelloID", float _floatWaitUpdateAI = 3f)
    {
        

            //if(activityAi == activityAI) return; //если активность повторяется
            _controlstringID_message = ID_dialogMessage;
            _controlFloatWaitUpdateAI = _floatWaitUpdateAI;
            _activityAI = activityAi;
            if (AI_enabled)
            {
                switch (_activityAI)
                {
                    case (ActivityAI.Work):
                    {
                        _localizationID = "Work";
                        _workActivity.WorkStartAtNPC(_characterNPC, transformTarget1, transformTarget2);
                        if (ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Work, _myCharacter) != null && ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Work, _myCharacter).Count > 0)
                        {

                            privateListActionsNPC =
                                ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Work, _myCharacter);
                            int random = Random.Range(0, privateListActionsNPC.Count);

                            if (CurrentActions != null)
                            {
                                CurrentActions.StopAction();
                            }

                            CurrentActions = privateListActionsNPC[random];

                            CurrentActions.UpdateNameCharacterAndStartFollow(_myCharacter);
                        }
                    }
                        break;
                    case (ActivityAI.Idle):
                    {
                        _localizationID = "Idle";
                        _idle.IdleStartAtNPC(_characterNPC, transformTarget1, transformTarget2);
                    
                        //испытание
                        if (ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Idle, _myCharacter) != null && ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Idle, _myCharacter).Count > 0)
                        {
                        
                            privateListActionsNPC =
                                ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Idle, _myCharacter);
                            int random = Random.Range(0, privateListActionsNPC.Count);

                            if (CurrentActions != null)
                            {
                                CurrentActions.StopAction();
                            }

                            CurrentActions = privateListActionsNPC[random];

                            CurrentActions.UpdateNameCharacterAndStartFollow(_myCharacter);
                        
                        } 
                    }
                        break;
                    case (ActivityAI.Follow):
                    {
                        _localizationID = "Follow";
                        _follow.FollowStartAtNPC(_characterNPC, transformTarget1, transformTarget2);
                        if (ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Follow, _myCharacter) != null && ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Follow, _myCharacter).Count > 0)
                        {

                            privateListActionsNPC =
                                ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Follow, _myCharacter);
                            int random = Random.Range(0, privateListActionsNPC.Count);

                            if (CurrentActions != null)
                            {
                                CurrentActions.StopAction();
                            }

                            CurrentActions = privateListActionsNPC[random];

                            CurrentActions.UpdateNameCharacterAndStartFollow(_myCharacter);
                        }
                    }
                        break;

                    case (ActivityAI.Talking):
                    {
                        _localizationID = "Talking";
                        _talking.TalkingStartAtNPC(_myCharacter, ID_dialogMessage);
                        EventGetActivityAI?.Invoke(ActivityAI.Talking); //активация FX эффекта у персонажа - разговор
                        if (ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Talking, _myCharacter) != null && ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Talking, _myCharacter).Count > 0)
                        {

                            privateListActionsNPC =
                                ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Talking, _myCharacter);
                            int random = Random.Range(0, privateListActionsNPC.Count);

                            if (CurrentActions != null)
                            {
                                CurrentActions.StopAction();
                            }

                            CurrentActions = privateListActionsNPC[random];

                            CurrentActions.UpdateNameCharacterAndStartFollow(_myCharacter);
                        }
                    }
                        break;

                    case (ActivityAI.Waiting):
                    {
                        _localizationID = "Waiting";
                        if (transformTarget1 != null) _waiting.StartWaiting(_characterNPC, transformTarget1);
                        if (transformTarget2 != null) _waiting.StartWaiting(_characterNPC, transformTarget2);
                        if (ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Waiting, _myCharacter) != null && ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Waiting, _myCharacter).Count > 0)
                        {

                            privateListActionsNPC =
                                ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Waiting, _myCharacter);
                            int random = Random.Range(0, privateListActionsNPC.Count);

                            if (CurrentActions != null)
                            {
                                CurrentActions.StopAction();
                            }

                            CurrentActions = privateListActionsNPC[random];

                            CurrentActions.UpdateNameCharacterAndStartFollow(_myCharacter);
                        }
                    }
                        break;

                    case (ActivityAI.Rest):
                    {
                        _localizationID = "Rest";
                        if (ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Rest, _myCharacter) != null && ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Rest, _myCharacter).Count > 0)
                        {

                            privateListActionsNPC =
                                ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Rest, _myCharacter);
                            int random = Random.Range(0, privateListActionsNPC.Count);

                            if (CurrentActions != null)
                            {
                                CurrentActions.StopAction();
                            }

                            CurrentActions = privateListActionsNPC[random];

                            CurrentActions.UpdateNameCharacterAndStartFollow(_myCharacter);
                        }
                        else
                        {
                            SwitchActivity(ActivityAI.Idle);
                        }
                    }
                        break;
                    case (ActivityAI.Eating):
                    {
                        _localizationID = "Eating";
                        if (ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Eating, _myCharacter) != null && ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Eating, _myCharacter).Count > 0)
                        {

                            privateListActionsNPC =
                                ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Eating, _myCharacter);
                            int random = Random.Range(0, privateListActionsNPC.Count);

                            if (CurrentActions != null)
                            {
                                CurrentActions.StopAction();
                            }

                            CurrentActions = privateListActionsNPC[random];

                            CurrentActions.UpdateNameCharacterAndStartFollow(_myCharacter);
                        }
                        else
                        {
                            SwitchActivity(ActivityAI.Idle);
                        }
                    }
                        break;
                    case (ActivityAI.Treated):
                    {
                        _localizationID = "Treated";
                        if (ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Treated, _myCharacter) != null && ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Treated, _myCharacter).Count > 0)
                        {

                            privateListActionsNPC =
                                ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Treated, _myCharacter);
                            int random = Random.Range(0, privateListActionsNPC.Count);

                            if (CurrentActions != null)
                            {
                                CurrentActions.StopAction();
                            }

                            CurrentActions = privateListActionsNPC[random];

                            CurrentActions.UpdateNameCharacterAndStartFollow(_myCharacter);
                        }
                        else
                        {
                            SwitchActivity(ActivityAI.Idle);
                        }
                    }
                        break;
                    case (ActivityAI.Escape):
                    {
                        _localizationID = "Escape";
                        if (ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Escape, _myCharacter) != null && ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Escape, _myCharacter).Count > 0)
                        {

                            privateListActionsNPC =
                                ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Escape, _myCharacter);
                            int random = Random.Range(0, privateListActionsNPC.Count);

                            if (CurrentActions != null)
                            {
                                CurrentActions.StopAction();
                            }

                            CurrentActions = privateListActionsNPC[random];

                            CurrentActions.UpdateNameCharacterAndStartFollow(_myCharacter);
                        }
                        else
                        {
                            SwitchActivity(ActivityAI.Idle);
                        }
                    }
                        break;
                    case (ActivityAI.SEX):
                    {
                        _localizationID = "SEX";
                        if (ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.SEX, _myCharacter) != null && ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.SEX, _myCharacter).Count > 0)
                        {

                            privateListActionsNPC =
                                ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.SEX, _myCharacter);
                            int random = Random.Range(0, privateListActionsNPC.Count);

                            if (CurrentActions != null)
                            {
                                CurrentActions.StopAction();
                            }

                            CurrentActions = privateListActionsNPC[random];

                            CurrentActions.UpdateNameCharacterAndStartFollow(_myCharacter);
                        }
                        else
                        {
                            SwitchActivity(ActivityAI.Idle);
                        }
                    }
                        break;
                    case (ActivityAI.Damn):
                    {
                        _localizationID = "Damn";
                        if (ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Damn, _myCharacter) != null && ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Damn, _myCharacter).Count > 0)
                        {

                            privateListActionsNPC =
                                ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Damn, _myCharacter);
                            int random = Random.Range(0, privateListActionsNPC.Count);

                            if (CurrentActions != null)
                            {
                                CurrentActions.StopAction();
                            }

                            CurrentActions = privateListActionsNPC[random];

                            CurrentActions.UpdateNameCharacterAndStartFollow(_myCharacter);
                        }
                        else
                        {
                            SwitchActivity(ActivityAI.Idle);
                        }
                    }
                        break;
                    case (ActivityAI.Fun):
                    {
                        _localizationID = "Fun";
                        if (ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Fun, _myCharacter) != null && ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Fun, _myCharacter).Count > 0)
                        {

                            privateListActionsNPC =
                                ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Fun, _myCharacter);
                            int random = Random.Range(0, privateListActionsNPC.Count);

                            if (CurrentActions != null)
                            {
                                CurrentActions.StopAction();
                            }

                            CurrentActions = privateListActionsNPC[random];

                            CurrentActions.UpdateNameCharacterAndStartFollow(_myCharacter);
                        }
                        else
                        {
                            SwitchActivity(ActivityAI.Idle);
                        }
                    }
                        break;
                    case (ActivityAI.HygieneUP):
                    {
                        _localizationID = "HygieneUP";
                        if (ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.HygieneUP, _myCharacter) != null && ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.HygieneUP, _myCharacter).Count > 0)
                        {

                            privateListActionsNPC =
                                ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.HygieneUP, _myCharacter);
                            int random = Random.Range(0, privateListActionsNPC.Count);

                            if (CurrentActions != null)
                            {
                                CurrentActions.StopAction();
                            }

                            CurrentActions = privateListActionsNPC[random];

                            CurrentActions.UpdateNameCharacterAndStartFollow(_myCharacter);
                        }
                        else
                        {
                            SwitchActivity(ActivityAI.Idle);
                        }
                    }
                        break;
                
                    case (ActivityAI.Learn):
                    {
                        _localizationID = "Learn";
                        if (ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Learn, _myCharacter) != null && ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Learn, _myCharacter).Count > 0)
                        {

                            privateListActionsNPC =
                                ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Learn, _myCharacter);
                            int random = Random.Range(0, privateListActionsNPC.Count);

                            if (CurrentActions != null)
                            {
                                CurrentActions.StopAction();
                            }

                            CurrentActions = privateListActionsNPC[random];

                            CurrentActions.UpdateNameCharacterAndStartFollow(_myCharacter);
                        }
                        else
                        {
                            SwitchActivity(ActivityAI.Idle);
                        }
                    }
                        break;
                
                    case (ActivityAI.Attack):
                    {
                        _localizationID = "Attack";
                        if (ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Attack, _myCharacter) != null && ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Attack, _myCharacter).Count > 0)
                        {

                            privateListActionsNPC =
                                ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Attack, _myCharacter);
                            int random = Random.Range(0, privateListActionsNPC.Count);

                            if (CurrentActions != null)
                            {
                                CurrentActions.StopAction();
                            }

                            CurrentActions = privateListActionsNPC[random];

                            CurrentActions.UpdateNameCharacterAndStartFollow(_myCharacter);
                        }
                        else
                        {
                            SwitchActivity(ActivityAI.Idle);
                        }
                    }
                        break;
                
                    case (ActivityAI.Defence):
                    {
                        _localizationID = "Defence";
                        if (ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Defence, _myCharacter) != null && ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Defence, _myCharacter).Count > 0)
                        {

                            privateListActionsNPC =
                                ScanActionsNPC.instance.GetActionsListNPC(ActivityAI.Defence, _myCharacter);
                            int random = Random.Range(0, privateListActionsNPC.Count);

                            if (CurrentActions != null)
                            {
                                CurrentActions.StopAction();
                            }

                            CurrentActions = privateListActionsNPC[random];

                            CurrentActions.UpdateNameCharacterAndStartFollow(_myCharacter);
                        }
                        else
                        {
                            SwitchActivity(ActivityAI.Idle);
                        }
                    }
                        break;
                }
        }
    }


    /// <summary>
    /// Переключить активность работы на требуемое
    /// </summary>
    /// <param name="workActivity"></param>
    internal void Switch_Work(IWorkActivity workActivity)
    {
        _workActivity = workActivity;
    }

    /// <summary>
    /// Переключить активность простоя
    /// </summary>
    /// <param name="idle"></param>
    internal void Switch_Idle(IIdle idle)
    {
        _idle = idle;
    }

    /// <summary>
    /// Переключить активность следования
    /// </summary>
    /// <param name="follow"></param>
    internal void Switch_Follow(IFollow follow)
    {
        _follow = follow;
    }

    /// <summary>
    /// Переключить активность разговора
    /// </summary>
    /// <param name="talking"></param>
    internal void Switch_Talking(ITalking talking)
    {
        _talking = talking;
    }

    /// <summary>
    /// Переключить активность ожидания
    /// </summary>
    /// <param name="waiting"></param>
    internal void Switch_Waiting(IWaiting waiting)
    {
        _waiting = waiting;
    }
    
    

    /// <summary>
    /// получить текущую активность
    /// </summary>
    /// <returns></returns>
    public ActivityAI GetActivity()
    {
        return _activityAI;
    }

    //тест контроля версий

    public virtual IEnumerator StateUpdate()
    {
        yield return null;
    }


    /// <summary>
    /// Запустить акшен, присоединенный к объекту, если он найден по ИД. Активности диалога (для игрока). Не забывать отключать АИ при необходимости чтобы АИ не мешал.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="charactersInitiator">чар с которым взаимодействует текущий чар</param>
    internal void StartInteraction(string id, Character charactersInitiator = null)
    {
        if(charactersInitiator !=null) _myCharacter.AddInteractionCharacter(charactersInitiator);
        
        foreach (var actions in _actionses)
        {
            if (String.Equals(actions.ID(), id))
            {
                actions.RunInteraction();
                break;
            }
        }
    }

    /// <summary>
    /// Отключить активность у чара по ИД активности (для игрока)
    /// </summary>
    /// <param name="id"></param>
    internal void EnableAction(string id)
    {
        StartCoroutine(EnableActionCoroutine(id));
    }
    
    

    private IEnumerator EnableActionCoroutine(string id)
    {
        yield return new WaitForSeconds(0.2f);
        foreach (var actions in _actionses)
        {
            if (String.Equals(actions.ID(), id))
            {
                actions.enabled = true;
                break;
            }
        }
    }

    internal void DisableAction(string id)
    {
        StartCoroutine(DisableActionCoroutine(id));
    }

    private IEnumerator DisableActionCoroutine(string id)
    {
        yield return new WaitForSeconds(0.2f);
        foreach (var actions in _actionses)
        {
           
            if (String.Equals(actions.ID(), id))
            {
                actions.enabled = false;
                break;
            }
        }
    }

    public delegate void GetActivityAI(ActivityAI activityAi);
    public event GetActivityAI EventGetActivityAI; //для системы частиц. 

    /// <summary>
    /// Текущее желание персонажа (какой активностью он хотел бы заняться)
    /// </summary>
    /// <returns></returns>
    internal virtual ActivityAI GetСurrentDesireActivityAi()
    {
        
        //Базовая цель - выжить
        
        if (_myCharacter.Health.Desire < 35) //если убивают
        {
            return ActivityAI.Escape; // бежать спасаться
        }
        
        
        if (_myCharacter.Health.Desire < 40 && _myCharacter.GetDiseases().Length > 0) //если болеет
        {
            return ActivityAI.Treated; //Лечиться
        }

        if (_myCharacter.Health.Desire < 40 && _myCharacter.Excitation.SensesLevel > 85) //если убивают
        {
            return ActivityAI.Escape; //бежать спасаться
        }
        
        
        //Первыми идут первичные потребности 2. Еда
        _myCharacter.Hunger.SensesLevel = _myCharacter.Food.Desire;
        if (_myCharacter.Hunger.SensesLevel < 40) return ActivityAI.Eating; //кушать
        
        //потом половое влечение
        if (_myCharacter.Libido.SensesLevel > 60) return ActivityAI.SEX; //заниматься

        if (_myCharacter.Aggressivity.SensesLevel > 60) return ActivityAI.Damn; //ругаться

        if (_myCharacter.Money.Res < 20 &&
            ScanActionsNPC.instance.GetNearActionNPC(ActivityAI.Work, _myCharacter) != null) return ActivityAI.Work;
        
        if (_myCharacter.Hygiene.Desire < 30) return ActivityAI.HygieneUP; //мыться 

        if (_myCharacter.Dissatisfaction.SensesLevel < 30) return ActivityAI.Fun; //если слабая удовлетворенность - должен развлечься
        if (_myCharacter.Communication.Desire < 30) return ActivityAI.Talking; //если слабое общение - должны общаться
        if (_myCharacter.Cognition.Desire < 10 && _myCharacter.Temperament.TemperamentStates() == TemperamentState.Nerd)
            return ActivityAI.Learn;
        if (_myCharacter.Apathy.SensesLevel > 50) return ActivityAI.Rest;
        
        return ActivityAI.Idle;
    }
    

}

public enum ActivityAI
{
    /// <summary>
    /// работать
    /// </summary>
    Work, 
    /// <summary>
    /// отдыхать
    /// </summary>
    Rest, 
    /// <summary>
    /// следовать за кем то, чем то
    /// </summary>
    Follow,
    /// <summary>
    /// простаивать
    /// </summary>
    Idle,
    /// <summary>
    /// Разговаривать
    /// </summary>
    Talking,
    /// <summary>
    /// Ожидать
    /// </summary>
    Waiting,
    /// <summary>
    /// Кушать
    /// </summary>
    Eating,
    /// <summary>
    /// Лечиться
    /// </summary>
    Treated,
    /// <summary>
    /// Спасаться
    /// </summary>
    Escape,
    /// <summary>
    /// Заняться сексом
    /// </summary>
    SEX,
    /// <summary>
    /// Ругаться
    /// </summary>
    Damn,
    /// <summary>
    /// Развлекаться
    /// </summary>
    Fun,
    //Мыться. Приводить себя в порядок
    HygieneUP,
    /// <summary>
    /// Учиться
    /// </summary>
    Learn,
    /// <summary>
    /// Нападать
    /// </summary>
    Attack,
    /// <summary>
    /// Защищаться
    /// </summary>
    Defence

}
