using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ��ͼ������,������ʾ��������ͼ
/// </summary>
public class ViewManager:Singleton<ViewManager>
{
    private Dictionary<string,GameObject> viewDic=new Dictionary<string,GameObject>();
    private GameObject canvas;

    /// <summary>
    ///  ��ʼ��UI������Canvas��UIcamera��EventSystem
    /// </summary>
    private void Init()
    {
        GameObject canvasObj = AddressableManager.Instance.LoadRes<GameObject>("Canvas");
        GameObject eventObj = AddressableManager.Instance.LoadRes<GameObject>("EventSystem");

        canvas=GameObject.Instantiate(canvasObj);
        GameObject eventSystem = GameObject.Instantiate(eventObj);
    }

    /// <summary>
    /// ��ʾ��ͼ
    /// </summary>
    /// <typeparam name="T">��Ӧ����ͼ��</typeparam>
    public void Show<T>() where T : class
    {
        if (canvas == null)
        {
            Init();
        }

        string name=typeof(T).Name;

        //����Ѽ��أ������
        if (viewDic.ContainsKey(name))
        {
            return ;
        }

        AddressableManager.Instance.LoadResAsync<GameObject>(name, (obj) =>
        {
            GameObject view = GameObject.Instantiate(obj);
            view.transform.SetParent(canvas.transform, false);
            viewDic[name] = view;
        });
    }

    /// <summary>
    /// ����ָ����ͼ
    /// </summary>
    /// <typeparam name="T">��Ӧ����ͼ��</typeparam>
    public void Hide<T>() where T : class
    {
        string name= typeof(T).Name;

        //û����ʾ����������
        if (!viewDic.ContainsKey(name))
        {
            return ;
        }

        //������ͼ
        GameObject.Destroy(viewDic[name]);
        viewDic.Remove(name);
    }

    //���������ͼ
    public void Clear()
    {
        foreach(string viewName in viewDic.Keys)
        {
            GameObject.Destroy(viewDic[viewName]);
            AddressableManager.Instance.Release<GameObject>(viewName);
        }
        viewDic.Clear();
    }
}
