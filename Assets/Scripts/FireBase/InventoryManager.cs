using Firebase.Database;
using Newtonsoft.Json;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{

    FirebaseDatabase database;
    DatabaseReference reference;
    UnityMainThreadDispatcher dispatcher;

    [Header("UI ")]
    [SerializeField] Text PotionCountText;
    [SerializeField] Text BombCountText;
    [SerializeField] Text TicketCountText;
    [SerializeField] Text MessageText;

    string userKey;

    Dictionary<string, int> inventory = new Dictionary<string, int>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        database = FirebaseDatabase.GetInstance(
            "https://test-78eaa-default-rtdb.asia-southeast1.firebasedatabase.app/"
            );

        reference = database.RootReference;
        dispatcher = UnityMainThreadDispatcher.Instance();

        LoadInventory();
    }

    void LoadInventory()
    {
        userKey = PlayerPrefs.GetString("UserKey");
        if (string.IsNullOrEmpty(userKey))
        {
            MessageText.text = "로그인 정보가 없습니다.";

            return;
        }

        reference.Child("UserInfo").Child(userKey).Child("Inventory").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                dispatcher.Enqueue(() =>
                {
                    MessageText.text = "인벤토리 불러오기 실패";
                });

                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Value == null)
                {
                    dispatcher.Enqueue(() =>
                    {
                        MessageText.text = "인벤토리 데이터가 없습니다.";
                    });

                    return;
                }

                string inventoryJson = snapshot.Value.ToString();

                inventory = JsonConvert.DeserializeObject<Dictionary<string, int>>(inventoryJson);

                dispatcher.Enqueue(() =>
                {
                    RefreshUI();
                    MessageText.text = "인벤토리 불러오기 완료 ";
                });
            }
        });
    }
    int GetltemCount(string itemName)
    {
        if (inventory.ContainsKey(itemName))
        {
            return inventory[itemName];
        }
        return 0;
    }
    void RefreshUI()
    {
        PotionCountText.text = "Potion:" + GetltemCount("Potion");
        BombCountText.text = "Bomb:" + GetltemCount("Bomb");
        TicketCountText.text = "Ticket :" + GetltemCount("Ticket");
    }

    void Useltem(string itemName)
    {
        if (!inventory.ContainsKey(itemName))
        {
            MessageText.text = itemName + "아이템이 없습니다.";

            return;
        }

        if (inventory[itemName] <= 0)
        {
            MessageText.text = itemName + "개수가 부족합니다. ";

            return;
        }

        inventory[itemName]--;
        RefreshUI();
        Savelnventory(itemName);
    }

    public void OnClickUsePotion()
    {
        Useltem("Potion");
    }

    public void OnClickUseBomb()
    {
        Useltem("Bomb");
    }

    public void OnClickUseTicket()
    {
        Useltem("Ticket");
    }
    void Savelnventory(string userItemname)
    {
        string inventoryJson = JsonConvert.SerializeObject(inventory);

        reference.Child("UserInfo").Child(userKey).Child("Inventory").SetValueAsync(inventoryJson).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                dispatcher.Enqueue(() =>
                {
                    MessageText.text = "인벤토리 저장 실패";
                });
                return;
            }
            if (task.IsCompleted)
            {
                dispatcher.Enqueue(() =>
                {
                    RefreshUI();
                    MessageText.text = userItemname + " 사용 완료";
                });
            }
        });
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
