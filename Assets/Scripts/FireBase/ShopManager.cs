using UnityEngine;
using Firebase.Database;
using UnityEngine.UI;
using PimDeWitte.UnityMainThreadDispatcher;
using Newtonsoft.Json;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    FirebaseDatabase database;
    DatabaseReference reference;
    UnityMainThreadDispatcher dispatcher;

    [Header("UI ")]
    [SerializeField] Text CoinText;
    [SerializeField] Text MessageText;

    string userkey;

    int currentCoin;
    Dictionary<string, int> inventory = new Dictionary<string, int>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        database = FirebaseDatabase.GetInstance(
            "https://test-78eaa-default-rtdb.asia-southeast1.firebasedatabase.app/"
            );

        reference = database.RootReference;
        dispatcher = UnityMainThreadDispatcher.Instance();

        LoadUserData();
    }

    // Update is called once per frame
    public void LoadUserData()
    {
        userkey = PlayerPrefs.GetString("UserKey");

        if (string.IsNullOrEmpty(userkey))
        {
            MessageText.text = "로그인 정보가 없습니다.";

            return;
        }

        reference.Child("UserInfo").Child(userkey).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                dispatcher.Enqueue(() =>
                {
                    MessageText.text = "유저 정보 불러오시 실패";
                });

                return;

            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                currentCoin = int.Parse(snapshot.Child("Coin").Value.ToString());
                string inventoryJson = snapshot.Child(" Inventory").Value.ToString();

                inventory = JsonConvert.DeserializeObject<Dictionary<string, int>>(inventoryJson);

                dispatcher.Enqueue(() =>
                {
                    RefreshUI();
                    MessageText.text = "유저 정보 불러오기 완료";
                });
            }
        });

    }

    void RefreshUI()
    {
        CoinText.text = "Coin : " + currentCoin;
    }
    public void OnClickBuyPotion()
    {
        Buyltem("Potion", 100);
    }
    public void OnClickBuyBomb()
    {
        Buyltem("Bomb", 50);
    }
    public void OnClickBuyTicket()
    {
        Buyltem("Ticket", 30);
    }

    void Buyltem(string itemName, int price)
    {
        if (currentCoin < price)
        {
            MessageText.text = "코인이 부족합니다.";

            return;
        }

        currentCoin -= price;

        if (inventory.ContainsKey(itemName))
        {
            inventory[itemName]++;
        }
        else
        {
            inventory.Add(itemName, 1);
        }

        SaveUserData(itemName);
    }

    void SaveUserData(string boughtltemName)
    {
        string inventoryJson = JsonConvert.SerializeObject(inventory);

        Dictionary<string, object> updateData = new Dictionary<string, object>();

        updateData["Coin"] = currentCoin;
        updateData["Inventory"] = inventoryJson;

        reference.Child("User Info").Child(userkey).UpdateChildrenAsync(updateData).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                dispatcher.Enqueue(() =>
                {
                    MessageText.text = "구매 저장 실패";
                });
                return;
            }

            if (task.IsCompleted)
            {
                dispatcher.Enqueue(() =>
                {
                    RefreshUI();
                    MessageText.text = boughtltemName + "구매 완료";
                });
            }
        });
    }
}
