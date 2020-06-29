using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour
{
    //public void InputFieldLostFocus()
    //{
    //    nameInputField.readOnly = true;
    //}
    //private void OnGUI()
    //{
    //    if (enterNamePanel.activeInHierarchy)
    //    {
    //        // Handle loss of focus & return
    //        if (nameInputField.readOnly)
    //        {
    //            if (!nameInputField.isFocused)
    //            {
    //                // Selecting must be done outside of the callback about InputField submission.
    //                nameInputField.Select();
    //                nameInputField.ActivateInputField();
    //            }
    //            else
    //            {
    //                // Updating carret must be done on subsequent updates after Selection callbacks worked
    //                nameInputField.MoveTextEnd(false);
    //                nameInputField.ForceLabelUpdate();
    //                nameInputField.readOnly = false;
    //                nameInputField.ForceLabelUpdate(); // ?
    //            }
    //        }
    //        else if (nameInputField.selectionAnchorPosition != nameInputField.selectionFocusPosition)
    //        {
    //            // Disallow selection
    //            nameInputField.selectionAnchorPosition = nameInputField.selectionFocusPosition;
    //            nameInputField.ForceLabelUpdate();
    //        }
    //    }
    //}
}
