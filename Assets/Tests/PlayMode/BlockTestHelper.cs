using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// Helper class that creates Block GameObjects configured for testing.
/// Unity MonoBehaviours cannot be instantiated with 'new', so we create
/// GameObjects and add the required components.
/// </summary>
public static class BlockTestHelper
{
    /// <summary>
    /// Creates a minimal GameObject with all components required by Block.Start().
    /// Block.Start() requires RectTransform, LayoutElement, and Shadow components.
    /// </summary>
    private static GameObject CreateBaseBlockGameObject(string name = "Block")
    {
        GameObject go = new GameObject(name);
        go.AddComponent<RectTransform>();
        go.AddComponent<LayoutElement>();
        go.AddComponent<Shadow>();
        return go;
    }

    /// <summary>
    /// Creates a SimpleInstructionBlock with the given instruction string.
    /// </summary>
    public static SimpleInstructionBlock CreateSimpleInstructionBlock(string instruction)
    {
        GameObject go = CreateBaseBlockGameObject("SimpleInstructionBlock");
        SimpleInstructionBlock block = go.AddComponent<SimpleInstructionBlock>();
        block.instruction = instruction;
        return block;
    }

    /// <summary>
    /// Creates a StartBlock.
    /// </summary>
    public static StartBlock CreateStartBlock()
    {
        GameObject go = CreateBaseBlockGameObject("StartBlock");
        StartBlock block = go.AddComponent<StartBlock>();
        return block;
    }

    /// <summary>
    /// Creates a ConstantNumberBlock with a Text child for NumberBlock.Start().
    /// </summary>
    public static ConstantNumberBlock CreateConstantNumberBlock(int number)
    {
        GameObject go = CreateBaseBlockGameObject("ConstantNumberBlock");

        // NumberBlock.Start() requires a child with a Text component
        GameObject textChild = new GameObject("Text");
        textChild.transform.SetParent(go.transform);
        textChild.AddComponent<Text>();

        ConstantNumberBlock block = go.AddComponent<ConstantNumberBlock>();
        block.number = number;
        return block;
    }

    /// <summary>
    /// Creates a CommandNumberBlock with a Text child.
    /// </summary>
    public static CommandNumberBlock CreateCommandNumberBlock(string command)
    {
        GameObject go = CreateBaseBlockGameObject("CommandNumberBlock");

        GameObject textChild = new GameObject("Text");
        textChild.transform.SetParent(go.transform);
        textChild.AddComponent<Text>();

        CommandNumberBlock block = go.AddComponent<CommandNumberBlock>();
        block.command = command;
        return block;
    }

    /// <summary>
    /// Creates a NumberOperationBlock.
    /// </summary>
    public static NumberOperationBlock CreateNumberOperationBlock(string operationString)
    {
        GameObject go = CreateBaseBlockGameObject("NumberOperationBlock");
        NumberOperationBlock block = go.AddComponent<NumberOperationBlock>();
        block.operationString = operationString;
        return block;
    }

    /// <summary>
    /// Creates a ConditionOperatorBlock.
    /// </summary>
    public static ConditionOperatorBlock CreateConditionOperatorBlock(string operationString)
    {
        GameObject go = CreateBaseBlockGameObject("ConditionOperatorBlock");
        ConditionOperatorBlock block = go.AddComponent<ConditionOperatorBlock>();
        block.operationString = operationString;
        return block;
    }

    /// <summary>
    /// Creates an IfThenBlock with a child tagged "Argument" for width calculation.
    /// </summary>
    public static IfThenBlock CreateIfThenBlock()
    {
        GameObject go = CreateBaseBlockGameObject("IfThenBlock");

        // IfThenBlock.Start() looks for a child tagged "Argument"
        GameObject argChild = new GameObject("Argument");
        argChild.tag = "Argument";
        argChild.AddComponent<RectTransform>();
        argChild.transform.SetParent(go.transform);

        IfThenBlock block = go.AddComponent<IfThenBlock>();
        return block;
    }

    /// <summary>
    /// Creates an IfThenElseBlock with required children and layout elements.
    /// </summary>
    public static IfThenElseBlock CreateIfThenElseBlock()
    {
        GameObject go = CreateBaseBlockGameObject("IfThenElseBlock");

        // IfThenBlock.Start() looks for a child tagged "Argument"
        GameObject argChild = new GameObject("Argument");
        argChild.tag = "Argument";
        argChild.AddComponent<RectTransform>();
        argChild.transform.SetParent(go.transform);

        // IfThenElseBlock requires upper and lower LayoutElements
        GameObject upperGO = new GameObject("Upper");
        upperGO.transform.SetParent(go.transform);
        LayoutElement upperLE = upperGO.AddComponent<LayoutElement>();

        GameObject lowerGO = new GameObject("Lower");
        lowerGO.transform.SetParent(go.transform);
        LayoutElement lowerLE = lowerGO.AddComponent<LayoutElement>();

        IfThenElseBlock block = go.AddComponent<IfThenElseBlock>();
        block.upperLayoutElement = upperLE;
        block.lowerLayoutElement = lowerLE;
        return block;
    }

    /// <summary>
    /// Creates a ForeverBlock with required children.
    /// </summary>
    public static ForeverBlock CreateForeverBlock()
    {
        GameObject go = CreateBaseBlockGameObject("ForeverBlock");

        GameObject argChild = new GameObject("Argument");
        argChild.tag = "Argument";
        argChild.AddComponent<RectTransform>();
        argChild.transform.SetParent(go.transform);

        ForeverBlock block = go.AddComponent<ForeverBlock>();
        return block;
    }

    /// <summary>
    /// Creates a WhileBlock with required children.
    /// </summary>
    public static WhileBlock CreateWhileBlock()
    {
        GameObject go = CreateBaseBlockGameObject("WhileBlock");

        GameObject argChild = new GameObject("Argument");
        argChild.tag = "Argument";
        argChild.AddComponent<RectTransform>();
        argChild.transform.SetParent(go.transform);

        WhileBlock block = go.AddComponent<WhileBlock>();
        return block;
    }

    /// <summary>
    /// Creates a ConditionBlock with required layout elements.
    /// </summary>
    public static ConditionBlock CreateConditionBlock(string operationString)
    {
        GameObject go = CreateBaseBlockGameObject("ConditionBlock");

        // ConditionBlock looks for children tagged "Argument" with LayoutElement
        GameObject argChild = new GameObject("Argument");
        argChild.tag = "Argument";
        argChild.AddComponent<RectTransform>();
        LayoutElement argLE = argChild.AddComponent<LayoutElement>();
        argLE.minWidth = 40f;
        argChild.transform.SetParent(go.transform);

        // Need left and right LayoutElements
        GameObject leftGO = new GameObject("Left");
        leftGO.transform.SetParent(go.transform);
        LayoutElement leftLE = leftGO.AddComponent<LayoutElement>();

        GameObject rightGO = new GameObject("Right");
        rightGO.transform.SetParent(go.transform);
        LayoutElement rightLE = rightGO.AddComponent<LayoutElement>();

        ConditionBlock block = go.AddComponent<ConditionBlock>();
        block.operationString = operationString;
        block.leftLayoutElement = leftLE;
        block.rightLayoutElement = rightLE;
        return block;
    }

    /// <summary>
    /// Initializes a block by calling Start() which sets up connections and components.
    /// In Unity PlayMode tests, Start() is called automatically on the next frame,
    /// but we call it manually for immediate initialization in tests.
    /// </summary>
    public static void InitializeBlock(Block block)
    {
        block.Start();
    }

    /// <summary>
    /// Cleans up a block's GameObject after a test.
    /// </summary>
    public static void DestroyBlock(Block block)
    {
        if (block != null && block.gameObject != null)
        {
            Object.DestroyImmediate(block.gameObject);
        }
    }
}
