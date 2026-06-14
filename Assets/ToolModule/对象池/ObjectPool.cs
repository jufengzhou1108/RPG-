using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

//�����
public class ObjectPool : SingletonAutoMono<ObjectPool>
{
    private Dictionary<string, Drawer> drawerDic = new();

    //�Ƿ���в㼶�Ż�
    public static bool isParent = false;

    /// <summary>
    /// ȡ����Ӧ������Դ
    /// </summary>
    /// <param name="key">addressable��</param>
    /// <returns></returns>
    public GameObject Pop(string key,int ceiling=10)
    {
        if (!drawerDic.ContainsKey(key))
        {
            InitDrawer(key, ceiling);
        }

        GameObject ans = drawerDic[key].Pop();
        ans.SetActive(true);
        return ans;
    }

    /// <summary>
    /// �����Ӧ�Ķ�����Դ
    /// </summary>
    /// <param name="obj">����ʵ��</param>
    /// <param name="ceiling">��������</param>
    public void Push(string key,GameObject obj,int ceiling=10)
    {
        if(!drawerDic.ContainsKey(key))
        {
            InitDrawer(key, ceiling);
        }

        drawerDic[key].Push(obj);
    }

    /// <summary>
    /// ��ն����
    /// </summary>
    public void Clear()
    {
        foreach(Drawer drawer in drawerDic.Values)
        {
            drawer.Clear();
            Destroy(drawer.gameObject);
            AddressableManager.Instance.Release<GameObject>(drawer.resName);
        }
        drawerDic.Clear();
    }

    public void InitDrawer(string key,int ceiling)
    {
        GameObject obj = new();
        if (isParent)
        {
            obj.transform.SetParent(this.transform);
        }
        obj.name = key;
        Drawer drawer= obj.AddComponent<Drawer>();
        drawer.ceiling = ceiling;
        drawer.obj= AddressableManager.Instance.LoadRes<GameObject>(key);
        drawer.resName = key;
        drawerDic.Add(key, drawer);
    }
}

//������
public class Drawer : MonoBehaviour 
{
    private Stack<GameObject> objStack = new();

    public string resName;
    public int ceiling;//��������
    public GameObject obj;//addressable���ص���Դ

    //ȡ��һ��ʧ��Ķ���
    public GameObject Pop()
    {
        if (objStack.Count < 1)
        {
            return Instantiate(obj);
        }

        return objStack.Pop();
    }

    //����һ������
    public void Push(GameObject obj)
    {
        if (objStack.Count >= ceiling)
        {
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        if (ObjectPool.isParent)
        {
            obj.transform.SetParent(this.transform);
        }
        objStack.Push(obj);
    }

    public void Clear()
    {
        foreach(GameObject obj in objStack)
        {
            Destroy(obj);
        }

        objStack.Clear();
    }
}

