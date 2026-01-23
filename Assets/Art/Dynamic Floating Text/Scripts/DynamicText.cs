using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DynamicText : MonoBehaviour
{
    private DynamicTextData data;
    private TextMeshProUGUI textObject;

    private bool initialised = false; // set to true after Initialise() is run
    private bool[] entered; // array to track progress of all entries
    private bool completeEntered = true; // above array is checked through every frame and if any of its values are false, this is set to false
    private bool exit = false; // used to track whether Exit() should be actively run
    private bool despawnStarted = false; // used to run the StartDespawn() coroutine once after initialisation

    // values used to change colour, size, position and exiting
    private float tColour = 0f;
    private float tSize = 0f;
    private float tPosition = 0f;
    private float tExit = 0f;

    // value used to calculate while lerping
    private Vector3 startPosition;
    private Vector3 startScale, startScaleZero;
    private Color startColour, startColourNoOpacity;
    // values used for bounce entries
    private float direction;

    // index values used in calculating colour changes and size changes
    private int totalColourIndex = 0;
    private int colourIndex = 0;
    private int nextColourIndex = 1;

    private int totalSizeIndex = 0;
    private int sizeIndex = 0;
    private int nextSizeIndex = 1;

    // === ADDED: cấu hình bay lên & trôi ngang nhẹ ===
    [SerializeField] private float riseSpeed = 1.1f; // tốc độ bay lên từ từ
    [SerializeField] private float sideDrift = 0.0f; // trôi ngang rất nhẹ (0 = tắt)

    void Update()
    {
        // Update() functions only run if Initialise() has been run
        if (initialised)
        {
            // check through the entered array. if any false values are found, set this to false
            completeEntered = true;
            for (int i = 0; i < entered.Length; i++)
            {
                if (!entered[i]) completeEntered = false;
            }

            // if no false values are found, entry must be complete so main functionality can be run
            if (completeEntered)
            {
                // if despawn timer has not been started, start it and some other once-off functions
                if (!despawnStarted)
                {
                    StartCoroutine(StartDespawn()); // start despawn timer

                    // if immediate colour alternation mode instead of gradient, this is calculated by running a single coroutine rather than actively calculating
                    if (data.colourAlternationMode == AlternationMode.Immediate)
                    {
                        StartCoroutine(ColourSwitch());
                    }

                    // same for size alternation
                    if (data.sizeAlternationMode == AlternationMode.Immediate)
                    {
                        StartCoroutine(SizeSwitch());
                    }
                }

                // if these modes are gradients, however, they are actively calculated
                if (data.colourAlternationMode == AlternationMode.Gradient && totalColourIndex < data.numberOfColourAlternations)
                {
                    ColourGradient();
                }

                if (data.sizeAlternationMode == AlternationMode.Gradient && totalSizeIndex < data.numberOfSizeAlternations)
                {
                    SizeGradient();
                }

                // calculate bounce position after having entered
                for (int i = 0; i < data.enterType.Length; i++)
                {
                    if (data.enterType[i] == EnterType.Bounce)
                    {
                        tPosition += Time.deltaTime / data.enterDuration;
                        Vector3 targetPosition = startPosition - new Vector3(direction, data.maxHeight, direction);
                        transform.position = Vector3.Slerp(startPosition, targetPosition, tPosition);
                    }
                }

                // run Exit() while exiting
                if (exit)
                {
                    Exit();
                }

                // === ADDED: luôn bay lên từ từ và trôi ngang rất nhẹ ===
                if (riseSpeed > 0f)
                    transform.position += (Vector3.up * riseSpeed + transform.right * sideDrift) * Time.deltaTime;

                // === ADDED: billboard nhìn về camera để dễ đọc ===
                if (DynamicTextManager.mainCamera)
                {
                    var cam = DynamicTextManager.mainCamera;
                    transform.LookAt(transform.position + (transform.position - cam.position));
                }
            }
            // run Enter() while entering
            else
            {
                Enter();
            }
        }
    }

    // function required to initialise the object, taking the desired text and date object as parameters
    public void Initialise(string _text, DynamicTextData _data)
    {
        // set the data object in this script to that which was passed
        data = _data;
        // change the placeholder text
        textObject = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        textObject.text = _text;

        // set the font
        if (data.font != null) textObject.font = data.font;

        // set bold, italics, underline, strikethrough
        if (data.bold) textObject.fontStyle = FontStyles.Bold;
        if (data.italic) textObject.fontStyle = FontStyles.Italic;
        if (data.underline) textObject.fontStyle = FontStyles.Underline;
        if (data.strikethrough) textObject.fontStyle = FontStyles.Strikethrough;

        if (data.colours.Length > 0) textObject.color = data.colours[0];
        if (data.sizes.Length > 0) textObject.transform.localScale = data.sizes[0] * Vector3.one;

        // assign start colour, scale and position
        startColour = textObject.color;
        startScale = textObject.transform.localScale;
        startPosition = transform.position;

        // choose a random direction, only used for Bounce entries
        direction = (Random.value - 0.5f) * data.maxDrift * 0.5f;

        // generate entered array by running through the total number of enter types in the data
        List<bool> enteredList = new List<bool>();
        for (int i = 0; i < data.enterType.Length; i++)
        {
            enteredList.Add(false);
        }
        entered = enteredList.ToArray();

        // run through each enter type and run the necessary one off functionality for each
        for (int i = 0; i < data.enterType.Length; i++)
        {
            if (data.enterType[i] == EnterType.Simple)
            {
                entered[i] = true;
            }
            if (data.enterType[i] == EnterType.Fade)
            {
                // set opacity to 0
                textObject.color = new Color(textObject.color.r, textObject.color.g, textObject.color.b, 0);
                startColourNoOpacity = textObject.color;
            }
            if (data.enterType[i] == EnterType.Pop)
            {
                // set scale to 0
                textObject.transform.localScale = new Vector3(0f, 0f, 0f);
                startScaleZero = textObject.transform.localScale;
            }
        }

        // mark initialised as true after everything is complete
        initialised = true;
    }

    IEnumerator ColourSwitch()
    {
        for (int i = 0; i < data.numberOfColourAlternations; i++)
        {
            if (colourIndex >= data.colours.Length) colourIndex = 0;
            if (nextColourIndex >= data.colours.Length) nextColourIndex = 0;
            yield return new WaitForSeconds(data.colourAlternationDuration);
            textObject.color = data.colours[nextColourIndex];
            totalColourIndex += 1;
            colourIndex += 1;
            nextColourIndex += 1;
        }
    }

    void ColourGradient()
    {
        tColour += Time.deltaTime / data.colourAlternationDuration;
        textObject.color = Color.Lerp(data.colours[colourIndex], data.colours[nextColourIndex], tColour);

        if (tColour >= 1)
        {
            totalColourIndex += 1;
            colourIndex += 1;
            nextColourIndex += 1;
            if (colourIndex >= data.colours.Length) colourIndex = 0;
            if (nextColourIndex >= data.colours.Length) nextColourIndex = 0;
            tColour = 0;
        }
    }

    IEnumerator SizeSwitch()
    {
        for (int i = 0; i < data.numberOfSizeAlternations; i++)
        {
            if (sizeIndex >= data.sizes.Length) sizeIndex = 0;
            if (nextSizeIndex >= data.sizes.Length) nextSizeIndex = 0;
            yield return new WaitForSeconds(data.sizeAlternationDuration);
            Vector3 newScale = new Vector3(
                data.sizes[nextSizeIndex],
                data.sizes[nextSizeIndex],
                data.sizes[nextSizeIndex]
            );
            textObject.transform.localScale = newScale;
            totalSizeIndex += 1;
            sizeIndex += 1;
            nextSizeIndex += 1;
        }
    }

    void SizeGradient()
    {
        tSize += Time.deltaTime / data.sizeAlternationDuration;
        textObject.transform.localScale =
            Vector3.Lerp(data.sizes[sizeIndex] * startScale, data.sizes[nextSizeIndex] * startScale, tSize);

        if (tSize >= 1)
        {
            totalSizeIndex += 1;
            sizeIndex += 1;
            nextSizeIndex += 1;
            if (sizeIndex >= data.sizes.Length) sizeIndex = 0;
            if (nextSizeIndex >= data.sizes.Length) nextSizeIndex = 0;
            tSize = 0;
        }
    }

    void Enter()
    {
        for (int i = 0; i < data.enterType.Length; i++)
        {
            if (!entered[i])
            {
                if (data.enterType[i] == EnterType.Fade)
                {
                    tColour += Time.deltaTime / data.enterDuration;
                    textObject.color = Color.Lerp(startColourNoOpacity, startColour, tColour);
                    if (tColour >= 1f)
                    {
                        tColour = 0f;
                        entered[i] = true;
                    }
                }
                if (data.enterType[i] == EnterType.Pop)
                {
                    tSize += Time.deltaTime / data.enterDuration;
                    Vector3 targetScale = startScale * data.popModifier;
                    textObject.transform.localScale = Vector3.Lerp(startScaleZero, targetScale, tSize);
                    if (tSize >= 1f)
                    {
                        tSize = 0f;
                        textObject.transform.localScale = startScale;
                        entered[i] = true;
                    }
                }
                if (data.enterType[i] == EnterType.Shift)
                {
                    tPosition += Time.deltaTime / data.enterDuration;
                    Vector3 targetPosition = startPosition + new Vector3(0f, data.maxHeight, 0f);
                    transform.position = Vector3.Lerp(startPosition, targetPosition, tPosition);
                    if (tPosition >= 1f)
                    {
                        tPosition = 0f;
                        transform.position = targetPosition;
                        entered[i] = true;
                    }
                }
                if (data.enterType[i] == EnterType.Bounce)
                {
                    tPosition += Time.deltaTime / data.enterDuration;
                    Vector3 targetPosition = startPosition + new Vector3(direction, data.maxHeight, direction);
                    transform.position = Vector3.Slerp(startPosition, targetPosition, tPosition);
                    if (tPosition >= 1f)
                    {
                        tPosition = 0f;
                        startPosition = transform.position; // set new start position to here
                        direction = -direction; // invert direction for on the way down
                        entered[i] = true;
                    }
                }
            }
        }
    }

    void Exit()
    {
        if (data.exitType == ExitType.Fade)
        {
            tColour += Time.deltaTime / data.exitDuration;
            textObject.color = Color.Lerp(startColour, startColourNoOpacity, tColour);
            if (tColour >= 1) Destroy(gameObject);
        }
        if (data.exitType == ExitType.Pop)
        {
            tSize += Time.deltaTime / data.exitDuration;
            Vector3 targetScale = startScale * data.popModifier;
            textObject.transform.localScale = Vector3.Lerp(targetScale, startScaleZero, tSize);
            if (tSize >= 1f) Destroy(gameObject);
        }
    }

    IEnumerator BlinkExit()
    {
        tExit = data.exitDuration * 3;
        Color currentColour = textObject.color;
        Color newColor = new Color(currentColour.r, currentColour.g, currentColour.b, 0f);
        exit = true;
        while (exit)
        {
            textObject.color = newColor;
            yield return new WaitForSeconds((data.exitDuration / tExit) * data.exitDuration);
            textObject.color = currentColour;
            tExit *= 2;
            yield return new WaitForSeconds((data.exitDuration / tExit) * data.exitDuration);
        }
    }

    IEnumerator BlinkExitDestruction()
    {
        yield return new WaitForSeconds(data.exitDuration);
        Destroy(gameObject);
    }

    IEnumerator StartDespawn()
    {
        despawnStarted = true;
        yield return new WaitForSeconds(data.lifetime);
        if (data.exitType == ExitType.Simple)
        {
            Destroy(gameObject);
        }
        if (data.exitType == ExitType.Fade || data.exitType == ExitType.Pop)
        {
            tColour = 0f;
            tSize = 0f;
            exit = true;
        }
        if (data.exitType == ExitType.Blink)
        {
            StartCoroutine(BlinkExit());
            StartCoroutine(BlinkExitDestruction());
        }
    }
}
