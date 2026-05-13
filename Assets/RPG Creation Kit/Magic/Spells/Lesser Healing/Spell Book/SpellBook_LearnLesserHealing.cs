using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class SpellBook_LearnLesserHealing : ItemScript
    {
        public override void OnReadBook(BookItem book)
        {
            base.OnReadBook(book);

            if(!SpellsKnowledge.Player.HasSpell("S_LesserHealing001"))
            {
                SpellsKnowledge.Player.LearnSpell("S_LesserHealing001");
                AlertMessage.instance.InitAlertMessage("You learn: Lesser Healing", 5.0f, false);
            }
        }
    }
}