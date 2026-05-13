using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Gamepad_ScrollBar : MonoBehaviour
{
    Scrollbar _scrollbar;
    float x, y;
    [SerializeField] private float scrollSpeed = 1;
    [SerializeField] private RectTransform view;
    [SerializeField] private RectTransform relative;

    // Start is called before the first frame update
    void Start()
    {
        _scrollbar = GetComponent<Scrollbar>();
    }

    public void INPUT_MoveAxis(InputAction.CallbackContext value)
    {
        Vector2 dValue = value.ReadValue<Vector2>();
        x = dValue.x;
        y = dValue.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _scrollbar.value += y * (scrollSpeed / (relative.rect.height - view.rect.height) * Time.deltaTime);
    }
}
