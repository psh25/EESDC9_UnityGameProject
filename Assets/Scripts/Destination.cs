using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Destination : Entity
{
    [SerializeField]private string nextSceneName;  //下一关的场景名称
    private bool finished = false;


    private void Update()       //检查是否完成关卡
    {
        if (GridManager == null)
        {
            return;
        }

        // 只遍历Tilemap上的有效格子，避免依赖固定宽高
        foreach (Vector2Int checkPos in GridManager.GetValidPositions())
        {
            if (GridManager.GetOccupant(checkPos) is Enemy || GridManager.GetOccupant(checkPos) is Firewall)
            {
                return;
            }
        }

        finished = true;
    }
    public override void Onhit(Vector2Int attackDirection)
    {
        if (finished == true)
        {
            SceneManager.LoadSceneAsync(nextSceneName,LoadSceneMode.Single);  //加载下一关场景
        }
        else
        {
            Debug.Log("关卡未完成");          //关卡未完成，不能进入下一关
                                              //Todo:可以考虑添加一些提示UI
            return;
        }
    }
}
