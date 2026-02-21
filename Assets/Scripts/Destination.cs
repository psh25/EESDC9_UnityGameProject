using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Destination : Entity
{
    private bool finished = false;
    private void Update()       //检查是否完成关卡
    {
        Vector2Int checkPos;
        for(int i = 0; i < GridManager.width; i++)
        {
            for(int j = 0; j < GridManager.height; j++)
            {
                checkPos = new Vector2Int(i, j);
                if (GridManager.GetOccupant(checkPos) is Enemy||GridManager.GetOccupant(checkPos) is Firewall)
                {
                    return;
                }
            }
        }
        finished = true;
    }
    public override void Onhit(Vector2Int attackDirection)
    {
        if (finished == true)
        {
            var activeScene = SceneManager.GetActiveScene();
            switch(activeScene.name)      //Todo:根据当前场景名称加载下一个场景
            {
                case "Game1":
                    SceneManager.LoadScene("StartScene");
                    break;
                default:
                    break;
            }
        }
        else
        {
            Debug.Log("关卡未完成");          //关卡未完成，不能进入下一关
                                              //Todo:可以考虑添加一些提示UI
            return;
        }
    }
}
