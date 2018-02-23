using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SettingsMenuHandler : MonoBehaviour {

    private Color disabledColor = new Color(1f,1f,1f,100f/255f);
    private Color enabledColor = new Color(52f/255f, 109f/255f, 25f/255f, 1f);

    public bool isChosen;
    public GameObject handler;

    void Start() {
        // Bounds an event trigger to the object
        EventTrigger trig = this.gameObject.AddComponent<EventTrigger>();
        // Creates an event entry
        EventTrigger.Entry entry = new EventTrigger.Entry();
        // Specifies the trigger type
        entry.eventID = EventTriggerType.PointerClick;
        // A different trigger is gettting bounded because MultiClick is allowed at Pnl_HarfBoyutu
        if(transform.parent.name == "Pnl_HarfBoyutu")
            entry.callback.AddListener((eventData) => { highlightPanel(enabledColor); });
        else
            entry.callback.AddListener((eventData)=> { highlightPanelAndDisableOthers(); });
        // Adds the trigger to the triggers list
        trig.triggers.Add(entry);


        handler = GameObject.Find("MenuHandler");
        if (handler == null)
            Debug.Log("MenuHandler can not be found!");
    }

    // Will highlight the selected option
    public void highlightPanel(Color _color) {
        Image img = this.GetComponent<Image>();
        // If the object is selected and user clicks it again deselects it
        if (img != null)
        {
            if (img.color == _color)
                _color = disabledColor;

            img.color = _color;

            if (transform.name.Contains("Boyut") || _color == enabledColor)
                updateMenuHandler();
        }
        else
        {
            Debug.Log("Error: Panel doesn't have an image!");
        }
    }

    // Will de-highlight all the options first then highlights the selected one
    public void highlightPanelAndDisableOthers() {

        Image img = this.GetComponent<Image>();
        if (img.color == enabledColor)
            return;

        Color color = disabledColor;

        for (int i = 0; i < transform.parent.transform.childCount; i++)
        {
            SettingsMenuHandler script = transform.parent.transform.GetChild(i).GetComponent<SettingsMenuHandler>();
            if (script != null) {
                script.highlightPanel(color);
            }
        }
        highlightPanel(enabledColor);
       //updateHandler();
    }


    public void updateMenuHandler() {
        MenuHandler updateScript = handler.GetComponent<MenuHandler>();

        if (updateScript == null) {
            Debug.Log("MenuHandler script is null.");
            return;
        }
        updateScript.updateSettings(transform.name);
    }

}
