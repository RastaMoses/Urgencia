using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using UnityEngine.UI;
using TMPro;
using System;
using RPGCreationKit.Player;
using UnityEngine.EventSystems;

namespace RPGCreationKit
{
    public class BookReaderManager : MonoBehaviour
    {
        public static BookReaderManager instance;
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Debug.LogError("Anomaly detected with the singleton pattern of 'BookReaderManager', are you using multiple BookReaderManager?");
        }


        private BookItem currentBook;
        public ItemInWorld currentBookInWorld;
        int totalPages;

        bool shouldDisableControls = false;


        [Header("Book References")]
        public GameObject backgroundUI;

        public GameObject bookUI;
        public Image openedBookImage;
        public TextMeshProUGUI bookLeftPage;
        public TextMeshProUGUI bookRightPage;

        [Space(5)]

        public TextMeshProUGUI bookLeftPageCount;
        public TextMeshProUGUI bookRightPageCount;

        [Space(5)]

        public GameObject bookPreviousButton;
        public GameObject bookNextButton;
        public GameObject bookTakeButton;

        [Header("Note References")]
        public GameObject noteUI;
        public Image openedNoteImage;
        public TextMeshProUGUI notePage;

        [Space(5)]

        public GameObject notePreviousButton;
        public GameObject noteNextButton;
        public GameObject noteTakeButton;

        [Header("Audio")]
        public AudioSource audioSource;

        [Space(5)]

        public AudioClip bookOpen;
        public AudioClip bookClose;
        public AudioClip bookNextPage;
        public AudioClip bookPreviousPage;

        bool openedFromInventory = false;
        public void ReadBook(BookItem _book, bool _shouldDisableControls, ItemInWorld _bookInWorld = null, bool _openedFromInventory = false)
        {
            //If was opened from the inventory, prevent the navigation in the inventory
            openedFromInventory = _openedFromInventory;

            if (openedFromInventory)
                EventSystem.current.sendNavigationEvents = false;

            shouldDisableControls = _shouldDisableControls;
            currentBook = _book;

            // ITEMSCRIPT_FUNC Run OnAdd
            if (!string.IsNullOrEmpty(_book.itemScript))
            {
                ItemScript iScript = (ItemScript)QuestScriptManager.instance.scriptsHolder.AddComponent(System.Type.GetType(_book.itemScript));
                iScript.OnReadBook(_book);
                Destroy(iScript);
            }

            if (_bookInWorld != null)
                currentBookInWorld = _bookInWorld;

            // Set Sprite 
            if (!currentBook.isNoteOrScroll)
            {
                if (currentBook.openedCoverSprite != null)
                    openedBookImage.sprite = currentBook.openedCoverSprite;
            } else
            {
                if (currentBook.openedCoverSprite != null)
                    openedNoteImage.sprite = currentBook.openedCoverSprite;
            }

            backgroundUI.SetActive(true);
            RckPlayer.instance.input.SwitchCurrentActionMap("BookUI");

            if (!_book.isNoteOrScroll)
            {
                bookUI.SetActive(true);

                // Init the book UI
                bookLeftPage.text = _book.BookText;
                bookRightPage.text = _book.BookText;

                bookLeftPageCount.text = bookLeftPage.pageToDisplay.ToString();
                bookRightPageCount.text = bookRightPage.pageToDisplay.ToString();

                bookLeftPage.ForceMeshUpdate();
                bookRightPage.ForceMeshUpdate();

                bookLeftPage.pageToDisplay = 1;
                bookRightPage.pageToDisplay = 2;

                totalPages = bookLeftPage.textInfo.pageCount;

                BookButtonsLogic();
                bookTakeButton.SetActive((currentBook.CantBeTaken || currentBookInWorld == null) ? false : true);

                if (_bookInWorld != null)
                    bookTakeButton.GetComponentInChildren<TextMeshProUGUI>().text = _bookInWorld.metadata.isOwned ? "STEAL" : "TAKE";

                audioSource.PlayOneShot(bookOpen);
            } else
            {
                noteUI.SetActive(true);

                // Init the book UI
                notePage.text = _book.BookText;

                notePage.ForceMeshUpdate();

                notePage.pageToDisplay = 1;

                totalPages = notePage.textInfo.pageCount;

                NoteButtonsLogic();
                noteTakeButton.SetActive((currentBook.CantBeTaken || currentBookInWorld == null) ? false : true);

                if(_bookInWorld != null)
                    noteTakeButton.GetComponentInChildren<TextMeshProUGUI>().text = _bookInWorld.metadata.isOwned ? "STEAL" : "TAKE";
                audioSource.PlayOneShot(bookOpen);
            }

            RckPlayer.instance.isReadingBook = true;

            if (RCKSettings.READING_BOOK_PAUSES_GAME)
                Time.timeScale = 0.0f;

            if(shouldDisableControls)
                RckPlayer.instance.EnableDisableControls(false);

            Invoke("OpenFix", .05f);
        }



        private void OpenFix()
        {
            if (currentBook == null)
                return;

            if (!currentBook.isNoteOrScroll)
            {
                bookLeftPageCount.text = bookLeftPage.pageToDisplay.ToString();
                bookRightPageCount.text = bookRightPage.pageToDisplay.ToString();

                bookLeftPage.pageToDisplay = 1;
                bookRightPage.pageToDisplay = 2;

                totalPages = bookLeftPage.textInfo.pageCount;

                BookButtonsLogic();
            } else
            {
                notePage.pageToDisplay = 1;
                totalPages = notePage.textInfo.pageCount;
                NoteButtonsLogic();
            }
        }

        public void NextPage()
        {
            if (!currentBook.isNoteOrScroll)
            {
                bookLeftPage.pageToDisplay = bookLeftPage.pageToDisplay + 2;
                bookRightPage.pageToDisplay = bookRightPage.pageToDisplay + 2;

                bookLeftPageCount.text = bookLeftPage.pageToDisplay.ToString();
                bookRightPageCount.text = bookRightPage.pageToDisplay.ToString();

                totalPages = bookLeftPage.textInfo.pageCount;

                BookButtonsLogic();

                audioSource.PlayOneShot(bookNextPage);
            } else
            {
                notePage.pageToDisplay = notePage.pageToDisplay + 1;

                totalPages = notePage.textInfo.pageCount;

                NoteButtonsLogic();

                audioSource.PlayOneShot(bookNextPage);
            }
        }


        public void PreviousPage()
        {
            if (!currentBook.isNoteOrScroll)
            {
                bookLeftPage.pageToDisplay = bookLeftPage.pageToDisplay - 2;
                bookRightPage.pageToDisplay = bookRightPage.pageToDisplay - 2;

                bookLeftPageCount.text = bookLeftPage.pageToDisplay.ToString();
                bookRightPageCount.text = bookRightPage.pageToDisplay.ToString();

                BookButtonsLogic();

                audioSource.PlayOneShot(bookPreviousPage);
            } else
            {
                notePage.pageToDisplay = notePage.pageToDisplay - 1;
                NoteButtonsLogic();
                audioSource.PlayOneShot(bookPreviousPage);
            }
        }

        public void TakeBook()
        {
            RckPlayer.instance.TakeItemInWorld(currentBookInWorld);
            CloseBook();
        }

        public void CloseBook()
        {
            bookLeftPage.text = "";
            bookRightPage.text = "";
            bookLeftPageCount.text = "";
            bookRightPageCount.text = "";
            bookNextButton.SetActive(false);
            bookPreviousButton.SetActive(false);

            backgroundUI.SetActive(false);
            noteUI.SetActive(false);
            bookUI.SetActive(false);

            RckPlayer.instance.isReadingBook = false;

            if (shouldDisableControls)
                RckPlayer.instance.EnableDisableControls(true);

            if(RCKSettings.READING_BOOK_PAUSES_GAME)
                Time.timeScale = 1.0f;

            currentBookInWorld = null;
            currentBook = null;

            audioSource.PlayOneShot(bookClose);

            if (openedFromInventory)
                EventSystem.current.sendNavigationEvents = true;

            if(openedFromInventory)
                RckPlayer.instance.input.SwitchCurrentActionMap("InventoryUI");
            else
                RckPlayer.instance.input.SwitchCurrentActionMap("Player");

            openedFromInventory = false;

        }


        private void BookButtonsLogic()
        {
            bookPreviousButton.SetActive(bookLeftPage.pageToDisplay > 1 ? true : false);
            bookNextButton.SetActive(bookRightPage.pageToDisplay < totalPages ? true : false);
        }

        private void NoteButtonsLogic()
        {
            notePreviousButton.SetActive(notePage.pageToDisplay > 1 ? true : false);
            noteNextButton.SetActive(notePage.pageToDisplay < totalPages ? true : false);
        }
    }
}