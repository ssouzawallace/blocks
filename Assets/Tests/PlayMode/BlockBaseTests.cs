using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// Tests for Block base class utility methods:
/// DescendingBlocks, ApplyDelta, Detach, HierarchyChanged, and IndentLogoCode.
/// </summary>
[TestFixture]
public class BlockBaseTests
{
    private SimpleInstructionBlock blockA;
    private SimpleInstructionBlock blockB;
    private SimpleInstructionBlock blockC;

    [SetUp]
    public void SetUp()
    {
        blockA = BlockTestHelper.CreateSimpleInstructionBlock("fd 100");
        BlockTestHelper.InitializeBlock(blockA);

        blockB = BlockTestHelper.CreateSimpleInstructionBlock("rt 90");
        BlockTestHelper.InitializeBlock(blockB);

        blockC = BlockTestHelper.CreateSimpleInstructionBlock("fd 50");
        BlockTestHelper.InitializeBlock(blockC);
    }

    [TearDown]
    public void TearDown()
    {
        BlockTestHelper.DestroyBlock(blockA);
        BlockTestHelper.DestroyBlock(blockB);
        BlockTestHelper.DestroyBlock(blockC);
    }

    #region DescendingBlocks Tests

    [Test]
    public void DescendingBlocks_SingleBlock_ReturnsSelf()
    {
        ArrayList result = blockA.DescendingBlocks();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(blockA, result[0]);
    }

    [Test]
    public void DescendingBlocks_TwoConnectedBlocks_ReturnsBoth()
    {
        // Connect blockA -> blockB via next connection
        blockA.transform.position = Vector3.zero;
        blockB.transform.position = Vector3.zero;

        var connNext = blockA.connections[1] as Block.Connection;
        connNext.TryAttachWithBlock(blockB);

        ArrayList result = blockA.DescendingBlocks();

        Assert.AreEqual(2, result.Count);
        Assert.Contains(blockA, result);
        Assert.Contains(blockB, result);
    }

    [Test]
    public void DescendingBlocks_ThreeChainedBlocks_ReturnsAll()
    {
        // Chain: blockA -> blockB -> blockC
        blockA.transform.position = Vector3.zero;
        blockB.transform.position = Vector3.zero;
        blockC.transform.position = Vector3.zero;

        var connNextA = blockA.connections[1] as Block.Connection;
        connNextA.TryAttachWithBlock(blockB);

        var connNextB = blockB.connections[1] as Block.Connection;
        connNextB.TryAttachWithBlock(blockC);

        ArrayList result = blockA.DescendingBlocks();

        Assert.AreEqual(3, result.Count);
        Assert.Contains(blockA, result);
        Assert.Contains(blockB, result);
        Assert.Contains(blockC, result);
    }

    [Test]
    public void DescendingBlocks_DisconnectedBlock_ExcludesIt()
    {
        // Only blockA -> blockB, blockC is not connected
        blockA.transform.position = Vector3.zero;
        blockB.transform.position = Vector3.zero;

        var connNext = blockA.connections[1] as Block.Connection;
        connNext.TryAttachWithBlock(blockB);

        ArrayList result = blockA.DescendingBlocks();

        Assert.AreEqual(2, result.Count);
        Assert.IsFalse(result.Contains(blockC));
    }

    #endregion

    #region ApplyDelta Tests

    [Test]
    public void ApplyDelta_SingleBlock_MovesBlock()
    {
        blockA.transform.position = new Vector3(10, 20, 0);

        blockA.ApplyDelta(new Vector2(5, -3));

        Assert.AreEqual(new Vector3(15, 17, 0), blockA.transform.position);
    }

    [Test]
    public void ApplyDelta_ChainedBlocks_MovesAllDescendants()
    {
        blockA.transform.position = new Vector3(0, 0, 0);
        blockB.transform.position = new Vector3(0, 0, 0);

        var connNext = blockA.connections[1] as Block.Connection;
        connNext.TryAttachWithBlock(blockB);

        Vector3 posA = blockA.transform.position;
        Vector3 posB = blockB.transform.position;

        blockA.ApplyDelta(new Vector2(10, 10));

        Assert.AreEqual(posA + new Vector3(10, 10, 0), blockA.transform.position);
        Assert.AreEqual(posB + new Vector3(10, 10, 0), blockB.transform.position);
    }

    #endregion

    #region Detach Tests

    [Test]
    public void Detach_DisconnectsBlockFromPrevious()
    {
        blockA.transform.position = Vector3.zero;
        blockB.transform.position = Vector3.zero;

        var connNext = blockA.connections[1] as Block.Connection;
        connNext.TryAttachWithBlock(blockB);

        Assert.IsNotNull(connNext.GetAttachedBlock());

        blockB.Detach();

        Assert.IsNull(connNext.GetAttachedBlock());
    }

    [Test]
    public void Detach_WhenNotConnected_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => blockA.Detach());
    }

    #endregion

    #region SetShadowActive Tests

    [Test]
    public void SetShadowActive_True_EnablesShadows()
    {
        blockA.SetShadowActive(true);

        foreach (Shadow shadow in blockA.shadows)
        {
            Assert.IsTrue(shadow.enabled);
        }
    }

    [Test]
    public void SetShadowActive_False_DisablesShadows()
    {
        blockA.SetShadowActive(true);
        blockA.SetShadowActive(false);

        foreach (Shadow shadow in blockA.shadows)
        {
            Assert.IsFalse(shadow.enabled);
        }
    }

    #endregion

    #region IndentLogoCode Tests (via reflection since it's private)

    [Test]
    public void IndentLogoCode_NoBlocks_ReturnsOriginalString()
    {
        string input = "fd 100";
        string result = InvokeIndentLogoCode(blockA, input);

        Assert.AreEqual("fd 100", result);
    }

    [Test]
    public void IndentLogoCode_SingleBracketBlock_IndentsContent()
    {
        string input = "forever [\nfd 100\n]";
        string result = InvokeIndentLogoCode(blockA, input);

        Assert.AreEqual("forever [\n    fd 100\n]", result);
    }

    [Test]
    public void IndentLogoCode_NestedBrackets_IndentsMultipleLevels()
    {
        string input = "forever [\nif (true) [\nfd 100\n]\n]";
        string result = InvokeIndentLogoCode(blockA, input);

        Assert.AreEqual("forever [\n    if (true) [\n        fd 100\n    ]\n]", result);
    }

    [Test]
    public void IndentLogoCode_EmptyString_ReturnsEmpty()
    {
        string result = InvokeIndentLogoCode(blockA, "");
        Assert.AreEqual("", result);
    }

    [Test]
    public void IndentLogoCode_NoBrackets_NoIndentation()
    {
        string input = "fd 100\nrt 90\nfd 100";
        string result = InvokeIndentLogoCode(blockA, input);

        Assert.AreEqual("fd 100\nrt 90\nfd 100", result);
    }

    /// <summary>
    /// Helper to invoke the private IndentLogoCode method via reflection.
    /// </summary>
    private static string InvokeIndentLogoCode(Block block, string code)
    {
        MethodInfo method = typeof(Block).GetMethod(
            "IndentLogoCode",
            BindingFlags.NonPublic | BindingFlags.Instance);
        return (string)method.Invoke(block, new object[] { code });
    }

    #endregion

    #region TryAttachInSomeConnectionWithBlock Tests

    [Test]
    public void TryAttachInSomeConnectionWithBlock_CompatibleBlocks_ReturnsTrue()
    {
        blockA.transform.position = Vector3.zero;
        blockB.transform.position = Vector3.zero;

        bool result = blockA.TryAttachInSomeConnectionWithBlock(blockB);

        // blockA has Male next, blockB has Female top - they should match
        Assert.IsTrue(result);
    }

    [Test]
    public void TryAttachInSomeConnectionWithBlock_SameBlock_ReturnsFalse()
    {
        // A block should not attach to itself (it's in its own DescendingBlocks)
        blockA.transform.position = Vector3.zero;

        bool result = blockA.TryAttachInSomeConnectionWithBlock(blockA);

        Assert.IsFalse(result);
    }

    [Test]
    public void TryAttachInSomeConnectionWithBlock_FarApart_ReturnsFalse()
    {
        blockA.transform.position = new Vector3(0, 0, 0);
        blockB.transform.position = new Vector3(1000, 1000, 0);

        bool result = blockA.TryAttachInSomeConnectionWithBlock(blockB);

        Assert.IsFalse(result);
    }

    #endregion

    #region singleInstructionHeight

    [Test]
    public void SingleInstructionHeight_IsExpectedValue()
    {
        Assert.AreEqual(37f, Block.singleInstructionHeight);
    }

    #endregion
}
