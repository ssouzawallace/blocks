using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// EditMode unit tests for Block.GetCode() code generation logic.
/// Tests verify that each block type produces the correct Logo-like code output
/// and that block chaining works correctly.
/// </summary>
[TestFixture]
public class BlockCodeGenerationTests
{
    private readonly List<GameObject> _createdObjects = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        foreach (var go in _createdObjects)
            Object.DestroyImmediate(go);
        _createdObjects.Clear();
    }

    /// <summary>
    /// Creates a GameObject with the minimum required components for a Block
    /// and manually calls Start() to initialize block connections.
    /// </summary>
    private T CreateBlock<T>() where T : Block
    {
        var go = new GameObject(typeof(T).Name);
        go.AddComponent<RectTransform>();
        go.AddComponent<LayoutElement>();
        _createdObjects.Add(go);

        var block = go.AddComponent<T>();
        block.Start();
        return block;
    }

    // -------------------------------------------------------------------------
    // SimpleInstructionBlock
    // connections: [0]=top(Female,Regular), [1]=next(Male,Regular)
    // -------------------------------------------------------------------------

    [Test]
    public void SimpleInstructionBlock_GetCode_ReturnsInstruction()
    {
        var block = CreateBlock<SimpleInstructionBlock>();
        block.instruction = "forward 10";

        Assert.AreEqual("forward 10", block.GetCode());
    }

    [Test]
    public void SimpleInstructionBlock_GetCode_NoInstruction_ReturnsEmpty()
    {
        var block = CreateBlock<SimpleInstructionBlock>();
        block.instruction = "";

        Assert.AreEqual("", block.GetCode());
    }

    [Test]
    public void SimpleInstructionBlock_GetCode_ChainsNextBlock()
    {
        var a = CreateBlock<SimpleInstructionBlock>();
        var b = CreateBlock<SimpleInstructionBlock>();
        a.instruction = "forward 10";
        b.instruction = "backward 5";

        // a.connections[1] = connectionNext, b.connections[0] = connectionTop
        a.connections[1].Attach(b, b.connections[0]);

        Assert.AreEqual("forward 10\nbackward 5", a.GetCode());
    }

    [Test]
    public void SimpleInstructionBlock_GetCode_ThreeBlockChain()
    {
        var a = CreateBlock<SimpleInstructionBlock>();
        var b = CreateBlock<SimpleInstructionBlock>();
        var c = CreateBlock<SimpleInstructionBlock>();
        a.instruction = "forward 10";
        b.instruction = "turn right 90";
        c.instruction = "forward 10";

        a.connections[1].Attach(b, b.connections[0]);
        b.connections[1].Attach(c, c.connections[0]);

        Assert.AreEqual("forward 10\nturn right 90\nforward 10", a.GetCode());
    }

    // -------------------------------------------------------------------------
    // StartBlock
    // connections: [0]=top(Female,Forbidden), [1]=next(Male,Regular)
    // -------------------------------------------------------------------------

    [Test]
    public void StartBlock_GetCode_EmptyProgram()
    {
        var start = CreateBlock<StartBlock>();

        Assert.AreEqual("to start\nend", start.GetCode());
    }

    [Test]
    public void StartBlock_GetCode_WithNextBlock()
    {
        var start = CreateBlock<StartBlock>();
        var instr = CreateBlock<SimpleInstructionBlock>();
        instr.instruction = "forward 10";

        // start.connections[1] = connectionNext
        start.connections[1].Attach(instr, instr.connections[0]);

        Assert.AreEqual("to start\nforward 10\nend", start.GetCode());
    }

    [Test]
    public void StartBlock_GetCode_WithMultipleInstructions()
    {
        var start = CreateBlock<StartBlock>();
        var instr1 = CreateBlock<SimpleInstructionBlock>();
        var instr2 = CreateBlock<SimpleInstructionBlock>();
        instr1.instruction = "forward 10";
        instr2.instruction = "backward 5";

        start.connections[1].Attach(instr1, instr1.connections[0]);
        instr1.connections[1].Attach(instr2, instr2.connections[0]);

        Assert.AreEqual("to start\nforward 10\nbackward 5\nend", start.GetCode());
    }

    // -------------------------------------------------------------------------
    // IfThenBlock
    // connections: [0]=top, [1]=then(Male,Regular), [2]=condition(Female,Logic), [3]=next
    // -------------------------------------------------------------------------

    [Test]
    public void IfThenBlock_GetCode_EmptyBlock()
    {
        var ifBlock = CreateBlock<IfThenBlock>();

        Assert.AreEqual("if () [\n\n]", ifBlock.GetCode());
    }

    [Test]
    public void IfThenBlock_GetCode_WithThenBlock()
    {
        var ifBlock = CreateBlock<IfThenBlock>();
        var instr = CreateBlock<SimpleInstructionBlock>();
        instr.instruction = "forward 10";

        // connections[1] = connectionThen
        ifBlock.connections[1].Attach(instr, instr.connections[0]);

        Assert.AreEqual("if () [\nforward 10\n]", ifBlock.GetCode());
    }

    [Test]
    public void IfThenBlock_GetCode_WithNextBlock()
    {
        var ifBlock = CreateBlock<IfThenBlock>();
        var next = CreateBlock<SimpleInstructionBlock>();
        next.instruction = "beep";

        // connections[3] = connectionNext
        ifBlock.connections[3].Attach(next, next.connections[0]);

        Assert.AreEqual("if () [\n\n]beep", ifBlock.GetCode());
    }

    // -------------------------------------------------------------------------
    // WhileBlock (extends IfThenBlock, same connection layout)
    // connections: [0]=top, [1]=then, [2]=condition, [3]=next
    // -------------------------------------------------------------------------

    [Test]
    public void WhileBlock_GetCode_Empty()
    {
        var whileBlock = CreateBlock<WhileBlock>();

        Assert.AreEqual("while () [\n\n]", whileBlock.GetCode());
    }

    [Test]
    public void WhileBlock_GetCode_WithBody()
    {
        var whileBlock = CreateBlock<WhileBlock>();
        var instr = CreateBlock<SimpleInstructionBlock>();
        instr.instruction = "forward 10";

        whileBlock.connections[1].Attach(instr, instr.connections[0]);

        Assert.AreEqual("while () [\nforward 10\n]", whileBlock.GetCode());
    }

    // -------------------------------------------------------------------------
    // ForeverBlock (extends IfThenBlock; removes condition and next connections)
    // connections after Start: [0]=top, [1]=then
    // -------------------------------------------------------------------------

    [Test]
    public void ForeverBlock_GetCode_Empty()
    {
        var forever = CreateBlock<ForeverBlock>();

        Assert.AreEqual("forever [\n]", forever.GetCode());
    }

    [Test]
    public void ForeverBlock_GetCode_WithBody()
    {
        var forever = CreateBlock<ForeverBlock>();
        var instr = CreateBlock<SimpleInstructionBlock>();
        instr.instruction = "forward 10";

        // connections[1] = connectionThen
        forever.connections[1].Attach(instr, instr.connections[0]);

        Assert.AreEqual("forever [forward 10\n]", forever.GetCode());
    }

    [Test]
    public void ForeverBlock_HasOnlyTwoConnections_AfterStart()
    {
        var forever = CreateBlock<ForeverBlock>();

        // ForeverBlock removes condition and next connections
        Assert.AreEqual(2, forever.connections.Count);
    }

    // -------------------------------------------------------------------------
    // IfThenElseBlock (extends IfThenBlock; inserts connectionElse at index 1)
    // connections: [0]=top, [1]=else(Male,Regular), [2]=then, [3]=condition, [4]=next
    // -------------------------------------------------------------------------

    [Test]
    public void IfThenElseBlock_GetCode_Empty()
    {
        var ifElse = CreateBlock<IfThenElseBlock>();

        Assert.AreEqual("if () [\n\n]else [\n\n]", ifElse.GetCode());
    }

    [Test]
    public void IfThenElseBlock_GetCode_ThenBranchOnly()
    {
        var ifElse = CreateBlock<IfThenElseBlock>();
        var thenInstr = CreateBlock<SimpleInstructionBlock>();
        thenInstr.instruction = "forward 10";

        // connections[2] = connectionThen
        ifElse.connections[2].Attach(thenInstr, thenInstr.connections[0]);

        Assert.AreEqual("if () [\nforward 10\n]else [\n\n]", ifElse.GetCode());
    }

    [Test]
    public void IfThenElseBlock_GetCode_BothBranches()
    {
        var ifElse = CreateBlock<IfThenElseBlock>();
        var thenInstr = CreateBlock<SimpleInstructionBlock>();
        var elseInstr = CreateBlock<SimpleInstructionBlock>();
        thenInstr.instruction = "forward 10";
        elseInstr.instruction = "backward 5";

        // connections[2] = connectionThen, connections[1] = connectionElse
        ifElse.connections[2].Attach(thenInstr, thenInstr.connections[0]);
        ifElse.connections[1].Attach(elseInstr, elseInstr.connections[0]);

        Assert.AreEqual("if () [\nforward 10\n]else [\nbackward 5\n]", ifElse.GetCode());
    }

    // -------------------------------------------------------------------------
    // NumberOperationBlock
    // connections: [0]=left(Female,Number), [1]=right(Female,Number)
    // -------------------------------------------------------------------------

    [Test]
    public void NumberOperationBlock_GetCode_Solo()
    {
        var op = CreateBlock<NumberOperationBlock>();
        op.operationString = "+";

        Assert.AreEqual("+", op.GetCode());
    }

    [Test]
    public void NumberOperationBlock_GetCode_WithRight()
    {
        var op = CreateBlock<NumberOperationBlock>();
        var right = CreateBlock<NumberOperationBlock>();
        op.operationString = "+";
        right.operationString = "5";

        // op.connections[1] = connectionRight, right.connections[0] = connectionLeft
        op.connections[1].Attach(right, right.connections[0]);

        Assert.AreEqual("+ 5", op.GetCode());
    }

    // -------------------------------------------------------------------------
    // ConditionOperatorBlock
    // connections: [0]=left(Female,Logic), [1]=right(Female,Logic)
    // -------------------------------------------------------------------------

    [Test]
    public void ConditionOperatorBlock_GetCode_Solo()
    {
        var op = CreateBlock<ConditionOperatorBlock>();
        op.operationString = "and";

        Assert.AreEqual("and", op.GetCode());
    }

    [Test]
    public void ConditionOperatorBlock_GetCode_WithRight()
    {
        var op1 = CreateBlock<ConditionOperatorBlock>();
        var op2 = CreateBlock<ConditionOperatorBlock>();
        op1.operationString = "and";
        op2.operationString = "true";

        op1.connections[1].Attach(op2, op2.connections[0]);

        Assert.AreEqual("and true", op1.GetCode());
    }

    // -------------------------------------------------------------------------
    // ConstantNumberBlock (extends NumberBlock)
    // connections: [0]=left(Male,Number), [1]=right(Male,Number)
    // -------------------------------------------------------------------------

    [Test]
    public void ConstantNumberBlock_GetCode_ReturnsNumber()
    {
        var cnb = CreateBlock<ConstantNumberBlock>();
        cnb.number = 42;

        Assert.AreEqual("42", cnb.GetCode());
    }

    [Test]
    public void ConstantNumberBlock_GetCode_ZeroDefault()
    {
        var cnb = CreateBlock<ConstantNumberBlock>();

        Assert.AreEqual("0", cnb.GetCode());
    }

    [Test]
    public void ConstantNumberBlock_GetCode_ChainedRight()
    {
        var cnb1 = CreateBlock<ConstantNumberBlock>();
        var cnb2 = CreateBlock<ConstantNumberBlock>();
        cnb1.number = 10;
        cnb2.number = 20;

        // cnb1.connections[1] = connectionRight, cnb2.connections[0] = connectionLeft
        cnb1.connections[1].Attach(cnb2, cnb2.connections[0]);

        Assert.AreEqual("10 20", cnb1.GetCode());
    }

    // -------------------------------------------------------------------------
    // CommandNumberBlock (extends NumberBlock)
    // connections: [0]=left(Male,Number), [1]=right(Male,Number)
    // -------------------------------------------------------------------------

    [Test]
    public void CommandNumberBlock_GetCode_ReturnsCommand()
    {
        var cmd = CreateBlock<CommandNumberBlock>();
        cmd.command = "sensor1";

        Assert.AreEqual("sensor1", cmd.GetCode());
    }

    [Test]
    public void CommandNumberBlock_GetCode_ChainedRight()
    {
        var cmd = CreateBlock<CommandNumberBlock>();
        var right = CreateBlock<ConstantNumberBlock>();
        cmd.command = "sensor1";
        right.number = 5;

        cmd.connections[1].Attach(right, right.connections[0]);

        Assert.AreEqual("sensor15", cmd.GetCode());
    }

    // -------------------------------------------------------------------------
    // SetSpeedBlock (extends BlockWithArgument extends SimpleInstructionBlock)
    // connections after Start: [0]=top, [1]=argument(Female,Number), [2]=next
    // -------------------------------------------------------------------------

    [Test]
    public void SetSpeedBlock_GetCode_WithoutArgument()
    {
        var speed = CreateBlock<SetSpeedBlock>();

        Assert.AreEqual("abcd, setpower \n", speed.GetCode());
    }

    [Test]
    public void SetSpeedBlock_GetCode_WithConstantArgument()
    {
        var speed = CreateBlock<SetSpeedBlock>();
        var num = CreateBlock<ConstantNumberBlock>();
        num.number = 50;

        // speed.connections[1] = argumentConnection
        speed.connections[1].Attach(num, num.connections[0]);

        Assert.AreEqual("abcd, setpower 50\n", speed.GetCode());
    }

    // -------------------------------------------------------------------------
    // Block.Connection detach/attach behaviour
    // -------------------------------------------------------------------------

    [Test]
    public void Connection_Attach_SetsAttachedBlock()
    {
        var a = CreateBlock<SimpleInstructionBlock>();
        var b = CreateBlock<SimpleInstructionBlock>();

        a.connections[1].Attach(b, b.connections[0]);

        Assert.AreEqual(b, a.connections[1].GetAttachedBlock());
        Assert.AreEqual(a, b.connections[0].GetAttachedBlock());
    }

    [Test]
    public void Connection_Detach_ClearsAttachedBlock()
    {
        var a = CreateBlock<SimpleInstructionBlock>();
        var b = CreateBlock<SimpleInstructionBlock>();

        a.connections[1].Attach(b, b.connections[0]);
        a.connections[1].Detach();

        Assert.IsNull(a.connections[1].GetAttachedBlock());
        Assert.IsNull(b.connections[0].GetAttachedBlock());
    }

    [Test]
    public void Connection_CannotAttachTwice()
    {
        var a = CreateBlock<SimpleInstructionBlock>();
        var b = CreateBlock<SimpleInstructionBlock>();
        var c = CreateBlock<SimpleInstructionBlock>();

        a.connections[1].Attach(b, b.connections[0]);
        // Second attach should be ignored since slot is already taken
        a.connections[1].Attach(c, c.connections[0]);

        Assert.AreEqual(b, a.connections[1].GetAttachedBlock());
        Assert.IsNull(c.connections[0].GetAttachedBlock());
    }
}
