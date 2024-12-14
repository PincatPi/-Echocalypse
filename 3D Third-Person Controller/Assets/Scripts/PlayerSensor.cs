using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSensor : MonoBehaviour
{
    public enum NextPlayerMovement
    {
        jump,
        climbLow,
        climbHigh,
        vault,
    }
    public NextPlayerMovement nextMovement = NextPlayerMovement.jump;
    //环境检测的最低高度，低于该高度的障碍物会被无视
    public float lowClimbHeight = 0.5f;
    //射线的长度
    public float checkDistance = 1f;
    public float climbAngle = 45f;
    private float climbDistance;
    //障碍物的法线信息
    public Vector3 climbHitNormal;
    //墙体顶端边缘信息
    public Vector3 ledge;
    //小于该高度，则玩家不能够通过
    public float bodyHeight = 1f;
    //高位攀爬的高度
    public float highClimbHeight = 1.6f;
    
    void Start()
    {
        climbDistance = Mathf.Cos(climbAngle) * checkDistance;
    }
    
    public NextPlayerMovement ClimbDetect(Transform playerTransform, Vector3 inputDirection, float offset)
    {
        //发出一条从玩家位置出发的高度为0.5的射线，只检测Ground层
        if (Physics.Raycast(playerTransform.position + Vector3.up * lowClimbHeight, Vector3.forward,
                out RaycastHit obsHit, checkDistance + offset, 1 << LayerMask.NameToLayer("Ground")))
        {
            climbHitNormal = obsHit.normal;
            //若玩家方向和墙体法线之间的夹角大于45度，则认为墙体不适合攀爬或翻阅
            //若玩家输入方向和墙体法线之间的夹角大于45度，也不能攀爬或翻阅
            //玩家没有输入时，面向墙壁也能够攀爬
            if (Vector3.Angle(-climbHitNormal, playerTransform.forward) > climbAngle || 
                Vector3.Angle(-climbHitNormal, inputDirection) > climbAngle)
            {
                return NextPlayerMovement.jump;
            }
            
            //距离玩家最近的位置是否有墙体，若有则可以攀爬
            //TODO: 此处的Ground层级判断可能会有问题
            //检测0.5米处
            if (Physics.Raycast(playerTransform.position + Vector3.up * lowClimbHeight, -climbHitNormal,
                    out RaycastHit firstWallHit, climbDistance + offset * Mathf.Cos(climbAngle), 1 << LayerMask.NameToLayer("Ground")))
            {
                //检测1.5米处
                if (Physics.Raycast(playerTransform.position + Vector3.up * (lowClimbHeight + bodyHeight),
                        -climbHitNormal,
                        out RaycastHit secondWallHit, climbDistance + offset * Mathf.Cos(climbAngle), 1 << LayerMask.NameToLayer("Ground")))
                {
                    //检测2.5米处
                    if (Physics.Raycast(playerTransform.position + Vector3.up * (lowClimbHeight + bodyHeight * 2),
                            -climbHitNormal,
                            out RaycastHit thirdWallHit, climbDistance + offset * Mathf.Cos(climbAngle), 1 << LayerMask.NameToLayer("Ground")))
                    {
                        //若3.5米处还有障碍物
                        if (Physics.Raycast(playerTransform.position + Vector3.up * (lowClimbHeight + bodyHeight * 3),
                                -climbHitNormal, climbDistance + offset * Mathf.Cos(climbAngle), 1 << LayerMask.NameToLayer("Ground")))
                        {
                            //进行跳跃
                            return NextPlayerMovement.jump;
                        }
                        //若3.5米处没有障碍物，则发射一条从3.5米处开始，向下发射，长度为bodyHeight（即1米）的射线，获取墙体的顶部信息
                        else if (Physics.Raycast(thirdWallHit.point + Vector3.up * bodyHeight, Vector3.down,
                                     out RaycastHit ledgeHit, bodyHeight, 1 << LayerMask.NameToLayer("Ground")))
                        {
                            //存储墙体顶部边缘信息
                            ledge = ledgeHit.point;
                            //高位攀爬
                            return NextPlayerMovement.climbHigh;
                        }
                    }
                    //2.5米处无障碍物
                    else if (Physics.Raycast(secondWallHit.point + Vector3.up * bodyHeight, Vector3.down,
                                 out RaycastHit ledgeHit, bodyHeight, 1 << LayerMask.NameToLayer("Ground")))
                    {
                        //存储墙体顶部边缘信息
                        ledge = ledgeHit.point;
                        //若高于highClimbHeight则高位攀爬
                        if (ledge.y - playerTransform.position.y > highClimbHeight)
                        {
                            return NextPlayerMovement.climbHigh;
                        }
                        //若低于highClimbHeight则判断墙体厚度
                        //大于0.2米厚的墙体低位攀爬
                        //TODO: 若墙体顶端不平整，则需要修改此处的射线长度（此时是bodyHeight）
                        else if (Physics.Raycast(secondWallHit.point + Vector3.up * bodyHeight - climbHitNormal * 0.2f,
                                     Vector3.down, bodyHeight, 1 << LayerMask.NameToLayer("Ground")))
                        {
                            return NextPlayerMovement.climbLow;
                        }
                        //小于0.2米厚的墙体进行翻阅
                        else
                        {
                            return NextPlayerMovement.vault;
                        }
                    }
                }
                else if(Physics.Raycast(firstWallHit.point + Vector3.up * bodyHeight, Vector3.down,
                            out RaycastHit ledgeHit, bodyHeight, 1 << LayerMask.NameToLayer("Ground")))
                {
                    //存储墙体顶部边缘信息
                    ledge = ledgeHit.point;
                    //大于0.2米厚的墙体低位攀爬
                    //TODO: 若墙体顶端不平整，则需要修改此处的射线长度（此时是bodyHeight）
                    if (Physics.Raycast(firstWallHit.point + Vector3.up * bodyHeight - climbHitNormal * 0.2f,
                                 Vector3.down, bodyHeight, 1 << LayerMask.NameToLayer("Ground")))
                    {
                        return NextPlayerMovement.climbLow;
                    }
                    //小于0.2米厚的墙体进行翻阅
                    else
                    {
                        return NextPlayerMovement.vault;
                    }
                }
            }
        }
        return NextPlayerMovement.jump;
    }
}
