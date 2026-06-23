using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class SpellBook_LearnFirebomb : ItemScript
    {
        public override void OnReadBook(BookItem book)
        {
            base.OnReadBook(book);

            if(!SpellsKnowledge.Player.HasSpell("S_Firebomb01"))
            {
                SpellsKnowledge.Player.LearnSpell("S_Firebomb01");
                AlertMessage.instance.InitAlertMessage("You learn: Firebomb", 5.0f, false);
            }
        }
    }
}