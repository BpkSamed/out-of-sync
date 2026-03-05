using UnityEngine;

public class item : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public GameObject worldItemPrefab;
    public void Item()
    {
        print("item dipakai");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector2 dropPosition = (Vector2)player.transform.position + new Vector2(0, 0);
            GameObject worldObj = Instantiate(worldItemPrefab, dropPosition, Quaternion.identity); // spawn item di dunia
            worldObj.tag = "item";
            WorldItem wi = worldObj.GetComponent<WorldItem>();
            if (wi != null)
            {
                wi.SetBisaDiambil(true); // default: item yang ditaruh bisa diambil lagi
            }
        }
        Destroy(this.gameObject);

    }
}