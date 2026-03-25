using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DialogDatabaseSO", menuName = "Dialog System/DialogDatabaseSO")]
public class DialogDatabaseSO : ScriptableObject
{
    public List<DialogSO>dialog = new List<DialogSO>();

    private Dictionary<int, DialogSO> dialogsByld;

    public void Initailize()
    {
        dialogsByld = new Dictionary<int, DialogSO>();

        foreach (var dialog in dialog)
        {
            if (dialog != null)
            {
                dialogsByld[dialog.id] = dialog;
            }
        }
    }

    public DialogSO GetDialodByld(int id)
    {
        if (dialogsByld == null)
            Initailize();

        if(dialogsByld.TryGetValue(id, out DialogSO dialog))
        {
            return dialog;
        }
        return null;
    }
}
