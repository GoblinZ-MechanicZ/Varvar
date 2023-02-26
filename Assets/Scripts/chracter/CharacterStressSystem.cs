using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CharacterStressSystem : MonoBehaviour
{
    [SerializeField] private CharacterController character;

    [SerializeField] private GlobalSettingsScriptable globalSettings;

    [SerializeField] private AudioSource heartSource;
    [SerializeField] private AudioSource themeLess50;
    [SerializeField] private AudioSource themeGreat50;
    [SerializeField] private Image stressVignette;
    [SerializeField] private Image panicImage;

    private float _stressLevel = 0f;
    private float _maxStressLevel => globalSettings.maxStressLevel;
    private float _stressPercent => _stressLevel / _maxStressLevel;

    private bool themeChanged = false;

    public void SetCharacter(CharacterController characterController)
    {
        character = characterController;
        character.OnSmetanaFound += OnSmetanaFound;
        character.OnAttacked += OnAttacked;
        character.OnPanic += OnPanic;

        StartCoroutine(DoUpdate());
    }

    private void OnPanic()
    {
        panicImage.color = new Color(0f, 0f, 0f, 1f);
    }

    private void OnSmetanaFound()
    {
        _stressLevel = 0f;
    }

    private void OnAttacked(EnemyType enemyType)
    {

    }

    private IEnumerator DoUpdate()
    {
        yield return new WaitForSeconds(2f);
        while (true)
        {
            if (!character.WasAttacked)
            {
                _stressLevel += (character.HasSmetana) ? globalSettings.stressPerSecond / 2 : globalSettings.stressPerSecond;
                _stressLevel += (character.RoomWithEnemy) ? ((character.HasSmetana) ? globalSettings.stressModWhenEnemyInRoom / 2 : globalSettings.stressModWhenEnemyInRoom) : 0;
                _stressLevel = Mathf.Clamp(_stressLevel, _stressLevel, _maxStressLevel);
            }

            UpdateAudioImage();

            yield return ChangeTheme();

            if (_stressLevel >= _maxStressLevel)
            {
                yield return character.DoPanic();
                yield return DoChill();
                yield return character.DoChill();
            }
            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator DoChill()
    {
        float timer = 0f;

        while (timer < globalSettings.stressChillTime)
        {
            timer += Time.deltaTime;
            _stressLevel -= Time.deltaTime * (globalSettings.stressAfterPanic / globalSettings.stressChillTime);
            panicImage.color = Color.Lerp(new Color(0f, 0f, 0f, 1f), new Color(0f, 0f, 0f, 0f), timer / globalSettings.stressChillTime);
            UpdateAudioImage();
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator ChangeTheme()
    {
        float timer = 0f;

        while (timer < 1f)
        {
            timer += Time.deltaTime;

            if (!themeChanged && _stressPercent >= .5f)
            {
                themeLess50.volume = Mathf.Lerp(0.3f, 0f, timer / 1f);
                themeGreat50.volume = Mathf.Lerp(0f, 0.3f, timer / 1f);
            }
            else if (themeChanged && _stressPercent < .5f)
            {
                themeLess50.volume = Mathf.Lerp(0f, 0.3f, timer / 1f);
                themeGreat50.volume = Mathf.Lerp(0.3f, 0f, timer / 1f);
            }

            yield return new WaitForEndOfFrame();
        }

        if (!themeChanged && _stressPercent >= .5f)
        {
            themeChanged = true;
        }
        else if (themeChanged && _stressPercent < .5f)
        {
            themeChanged = false;
        }
    }

    private void UpdateAudioImage()
    {
        heartSource.volume = _stressPercent / globalSettings.heartVolumeReduction;
        stressVignette.color = new Color(1, 1, 1, _stressPercent);
    }
}