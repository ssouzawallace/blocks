using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// Tests for GetCode() on all block types.
/// Verifies that each block correctly generates its code string,
/// both in isolation and when connected to other blocks.
/// </summary>
[TestFixture]
public class GetCodeTests
{
    #region StartBlock

    [Test]
    public void StartBlock_GetCode_NoChildren_ReturnsStartEnd()
    {
        var block = BlockTestHelper.CreateStartBlock();
        BlockTestHelper.InitializeBlock(block);

        string code = block.GetCode();

        Assert.AreEqual("to start\nend", code);

        BlockTestHelper.DestroyBlock(block);
    }

    [Test]
    public void StartBlock_GetCode_WithChild_IncludesChildCode()
    {
        var startBlock = BlockTestHelper.CreateStartBlock();
        BlockTestHelper.InitializeBlock(startBlock);

        var childBlock = BlockTestHelper.CreateSimpleInstructionBlock("fd 100");
        BlockTestHelper.InitializeBlock(childBlock);

        // Connect: startBlock.next -> childBlock.top
        startBlock.transform.position = Vector3.zero;
        childBlock.transform.position = Vector3.zero;

        var connNext = startBlock.connections[1] as Block.Connection;
        connNext.TryAttachWithBlock(childBlock);

        string code = startBlock.GetCode();

        Assert.AreEqual("to start\nfd 100\nend", code);

        BlockTestHelper.DestroyBlock(startBlock);
        BlockTestHelper.DestroyBlock(childBlock);
    }

    #endregion

    #region SimpleInstructionBlock

    [Test]
    public void SimpleInstructionBlock_GetCode_ReturnsInstruction()
    {
        var block = BlockTestHelper.CreateSimpleInstructionBlock("fd 100");
        BlockTestHelper.InitializeBlock(block);

        string code = block.GetCode();

        Assert.AreEqual("fd 100", code);

        BlockTestHelper.DestroyBlock(block);
    }

    [Test]
    public void SimpleInstructionBlock_GetCode_WithNext_ChainsCode()
    {
        var block1 = BlockTestHelper.CreateSimpleInstructionBlock("fd 100");
        BlockTestHelper.InitializeBlock(block1);

        var block2 = BlockTestHelper.CreateSimpleInstructionBlock("rt 90");
        BlockTestHelper.InitializeBlock(block2);

        block1.transform.position = Vector3.zero;
        block2.transform.position = Vector3.zero;

        var connNext = block1.connections[1] as Block.Connection;
        connNext.TryAttachWithBlock(block2);

        string code = block1.GetCode();

        Assert.AreEqual("fd 100\nrt 90", code);

        BlockTestHelper.DestroyBlock(block1);
        BlockTestHelper.DestroyBlock(block2);
    }

    [Test]
    public void SimpleInstructionBlock_GetCode_ThreeChained_ChainsAll()
    {
        var block1 = BlockTestHelper.CreateSimpleInstructionBlock("fd 100");
        BlockTestHelper.InitializeBlock(block1);

        var block2 = BlockTestHelper.CreateSimpleInstructionBlock("rt 90");
        BlockTestHelper.InitializeBlock(block2);

        var block3 = BlockTestHelper.CreateSimpleInstructionBlock("fd 50");
        BlockTestHelper.InitializeBlock(block3);

        block1.transform.position = Vector3.zero;
        block2.transform.position = Vector3.zero;
        block3.transform.position = Vector3.zero;

        (block1.connections[1] as Block.Connection).TryAttachWithBlock(block2);
        (block2.connections[1] as Block.Connection).TryAttachWithBlock(block3);

        string code = block1.GetCode();

        Assert.AreEqual("fd 100\nrt 90\nfd 50", code);

        BlockTestHelper.DestroyBlock(block1);
        BlockTestHelper.DestroyBlock(block2);
        BlockTestHelper.DestroyBlock(block3);
    }

    #endregion

    #region ConstantNumberBlock

    [Test]
    public void ConstantNumberBlock_GetCode_ReturnsNumber()
    {
        var block = BlockTestHelper.CreateConstantNumberBlock(42);
        BlockTestHelper.InitializeBlock(block);

        string code = block.GetCode();

        Assert.AreEqual("42", code);

        BlockTestHelper.DestroyBlock(block);
    }

    [Test]
    public void ConstantNumberBlock_GetCode_Zero_ReturnsZero()
    {
        var block = BlockTestHelper.CreateConstantNumberBlock(0);
        BlockTestHelper.InitializeBlock(block);

        string code = block.GetCode();

        Assert.AreEqual("0", code);

        BlockTestHelper.DestroyBlock(block);
    }

    [Test]
    public void ConstantNumberBlock_GetCode_NegativeNumber()
    {
        var block = BlockTestHelper.CreateConstantNumberBlock(-5);
        BlockTestHelper.InitializeBlock(block);

        string code = block.GetCode();

        Assert.AreEqual("-5", code);

        BlockTestHelper.DestroyBlock(block);
    }

    [Test]
    public void ConstantNumberBlock_GetCode_WithRightAttached_ChainsCode()
    {
        var block1 = BlockTestHelper.CreateConstantNumberBlock(10);
        BlockTestHelper.InitializeBlock(block1);

        var opBlock = BlockTestHelper.CreateNumberOperationBlock("+");
        BlockTestHelper.InitializeBlock(opBlock);

        // ConstantNumberBlock: connections[0]=Left(Male/Number), connections[1]=Right(Male/Number)
        // NumberOperationBlock: connections[0]=Left(Female/Number), connections[1]=Right(Female/Number)
        // block1.Right(Male) should match opBlock.Left(Female)
        block1.transform.position = Vector3.zero;
        opBlock.transform.position = Vector3.zero;

        var connRight = block1.connections[1] as Block.Connection;
        connRight.TryAttachWithBlock(opBlock);

        string code = block1.GetCode();

        Assert.AreEqual("10 +", code);

        BlockTestHelper.DestroyBlock(block1);
        BlockTestHelper.DestroyBlock(opBlock);
    }

    #endregion

    #region CommandNumberBlock

    [Test]
    public void CommandNumberBlock_GetCode_ReturnsCommand()
    {
        var block = BlockTestHelper.CreateCommandNumberBlock("sensor1");
        BlockTestHelper.InitializeBlock(block);

        string code = block.GetCode();

        Assert.AreEqual("sensor1", code);

        BlockTestHelper.DestroyBlock(block);
    }

    [Test]
    public void CommandNumberBlock_GetCode_EmptyCommand()
    {
        var block = BlockTestHelper.CreateCommandNumberBlock("");
        BlockTestHelper.InitializeBlock(block);

        string code = block.GetCode();

        Assert.AreEqual("", code);

        BlockTestHelper.DestroyBlock(block);
    }

    #endregion

    #region NumberOperationBlock

    [Test]
    public void NumberOperationBlock_GetCode_NoAttachment_ReturnsOperation()
    {
        var block = BlockTestHelper.CreateNumberOperationBlock("+");
        BlockTestHelper.InitializeBlock(block);

        string code = block.GetCode();

        Assert.AreEqual("+", code);

        BlockTestHelper.DestroyBlock(block);
    }

    [Test]
    public void NumberOperationBlock_GetCode_Subtraction()
    {
        var block = BlockTestHelper.CreateNumberOperationBlock("-");
        BlockTestHelper.InitializeBlock(block);

        Assert.AreEqual("-", block.GetCode());

        BlockTestHelper.DestroyBlock(block);
    }

    [Test]
    public void NumberOperationBlock_GetCode_Multiplication()
    {
        var block = BlockTestHelper.CreateNumberOperationBlock("*");
        BlockTestHelper.InitializeBlock(block);

        Assert.AreEqual("*", block.GetCode());

        BlockTestHelper.DestroyBlock(block);
    }

    [Test]
    public void NumberOperationBlock_GetCode_Division()
    {
        var block = BlockTestHelper.CreateNumberOperationBlock("/");
        BlockTestHelper.InitializeBlock(block);

        Assert.AreEqual("/", block.GetCode());

        BlockTestHelper.DestroyBlock(block);
    }

    #endregion

    #region ConditionOperatorBlock

    [Test]
    public void ConditionOperatorBlock_GetCode_NoAttachment_ReturnsOperation()
    {
        var block = BlockTestHelper.CreateConditionOperatorBlock("and");
        BlockTestHelper.InitializeBlock(block);

        string code = block.GetCode();

        Assert.AreEqual("and", code);

        BlockTestHelper.DestroyBlock(block);
    }

    [Test]
    public void ConditionOperatorBlock_GetCode_Or()
    {
        var block = BlockTestHelper.CreateConditionOperatorBlock("or");
        BlockTestHelper.InitializeBlock(block);

        Assert.AreEqual("or", block.GetCode());

        BlockTestHelper.DestroyBlock(block);
    }

    #endregion

    #region ConditionBlock

    [Test]
    public void ConditionBlock_GetCode_NoAttachments_ReturnsOperationOnly()
    {
        var block = BlockTestHelper.CreateConditionBlock(">");
        BlockTestHelper.InitializeBlock(block);

        string code = block.GetCode();

        // No left or right number attached, just the operator with spaces
        Assert.AreEqual(" > ", code);

        BlockTestHelper.DestroyBlock(block);
    }

    [Test]
    public void ConditionBlock_GetCode_Equals()
    {
        var block = BlockTestHelper.CreateConditionBlock("=");
        BlockTestHelper.InitializeBlock(block);

        Assert.AreEqual(" = ", block.GetCode());

        BlockTestHelper.DestroyBlock(block);
    }

    [Test]
    public void ConditionBlock_GetCode_LessThan()
    {
        var block = BlockTestHelper.CreateConditionBlock("<");
        BlockTestHelper.InitializeBlock(block);

        Assert.AreEqual(" < ", block.GetCode());

        BlockTestHelper.DestroyBlock(block);
    }

    #endregion

    #region IfThenBlock

    [Test]
    public void IfThenBlock_GetCode_Empty_ReturnsStructure()
    {
        var block = BlockTestHelper.CreateIfThenBlock();
        BlockTestHelper.InitializeBlock(block);

        string code = block.GetCode();

        Assert.AreEqual("if () [\n\n]", code);

        BlockTestHelper.DestroyBlock(block);
    }

    #endregion

    #region IfThenElseBlock

    [Test]
    public void IfThenElseBlock_GetCode_Empty_ReturnsFullStructure()
    {
        var block = BlockTestHelper.CreateIfThenElseBlock();
        BlockTestHelper.InitializeBlock(block);

        string code = block.GetCode();

        Assert.AreEqual("if () [\n\n]else [\n\n]", code);

        BlockTestHelper.DestroyBlock(block);
    }

    #endregion

    #region ForeverBlock

    [Test]
    public void ForeverBlock_GetCode_Empty_ReturnsForeverStructure()
    {
        var block = BlockTestHelper.CreateForeverBlock();
        BlockTestHelper.InitializeBlock(block);

        string code = block.GetCode();

        Assert.AreEqual("forever [\n]", code);

        BlockTestHelper.DestroyBlock(block);
    }

    [Test]
    public void ForeverBlock_ConnectionsRemoved_NoConditionOrNext()
    {
        var block = BlockTestHelper.CreateForeverBlock();
        BlockTestHelper.InitializeBlock(block);

        // ForeverBlock removes connectionCondition and connectionNext
        // Should only have connectionTop and connectionThen
        Assert.AreEqual(2, block.connections.Count);

        BlockTestHelper.DestroyBlock(block);
    }

    #endregion

    #region WhileBlock

    [Test]
    public void WhileBlock_GetCode_Empty_ReturnsWhileStructure()
    {
        var block = BlockTestHelper.CreateWhileBlock();
        BlockTestHelper.InitializeBlock(block);

        string code = block.GetCode();

        Assert.AreEqual("while () [\n\n]", code);

        BlockTestHelper.DestroyBlock(block);
    }

    #endregion

    #region SetSpeedBlock

    [Test]
    public void SetSpeedBlock_GetCode_NoArgument_ReturnsBaseCommand()
    {
        // SetSpeedBlock extends BlockWithArgument which needs an "Argument" tagged child
        GameObject go = new GameObject("SetSpeedBlock");
        go.AddComponent<RectTransform>();
        go.AddComponent<LayoutElement>();
        go.AddComponent<Shadow>();

        GameObject argChild = new GameObject("Argument");
        argChild.tag = "Argument";
        argChild.AddComponent<RectTransform>();
        argChild.transform.SetParent(go.transform);

        SetSpeedBlock block = go.AddComponent<SetSpeedBlock>();
        block.instruction = "";
        BlockTestHelper.InitializeBlock(block);

        string code = block.GetCode();

        Assert.AreEqual("abcd, setpower \n", code);

        BlockTestHelper.DestroyBlock(block);
    }

    #endregion

    #region Integration: StartBlock with multiple children

    [Test]
    public void StartBlock_WithTwoInstructions_GeneratesFullProgram()
    {
        var startBlock = BlockTestHelper.CreateStartBlock();
        BlockTestHelper.InitializeBlock(startBlock);

        var instr1 = BlockTestHelper.CreateSimpleInstructionBlock("fd 100");
        BlockTestHelper.InitializeBlock(instr1);

        var instr2 = BlockTestHelper.CreateSimpleInstructionBlock("rt 90");
        BlockTestHelper.InitializeBlock(instr2);

        startBlock.transform.position = Vector3.zero;
        instr1.transform.position = Vector3.zero;
        instr2.transform.position = Vector3.zero;

        // Chain: startBlock -> instr1 -> instr2
        (startBlock.connections[1] as Block.Connection).TryAttachWithBlock(instr1);
        (instr1.connections[1] as Block.Connection).TryAttachWithBlock(instr2);

        string code = startBlock.GetCode();

        Assert.AreEqual("to start\nfd 100\nrt 90\nend", code);

        BlockTestHelper.DestroyBlock(startBlock);
        BlockTestHelper.DestroyBlock(instr1);
        BlockTestHelper.DestroyBlock(instr2);
    }

    [Test]
    public void StartBlock_WithWhileLoop_GeneratesNestedCode()
    {
        var startBlock = BlockTestHelper.CreateStartBlock();
        BlockTestHelper.InitializeBlock(startBlock);

        var whileBlock = BlockTestHelper.CreateWhileBlock();
        BlockTestHelper.InitializeBlock(whileBlock);

        startBlock.transform.position = Vector3.zero;
        whileBlock.transform.position = Vector3.zero;

        // startBlock.connections[1] is Male/Regular (next)
        // whileBlock.connections[0] is Female/Regular (top)
        (startBlock.connections[1] as Block.Connection).TryAttachWithBlock(whileBlock);

        string code = startBlock.GetCode();

        Assert.AreEqual("to start\nwhile () [\n\n]\nend", code);

        BlockTestHelper.DestroyBlock(startBlock);
        BlockTestHelper.DestroyBlock(whileBlock);
    }

    #endregion
}
