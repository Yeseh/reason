using Reason.Client.Script;

namespace Reason.Client.Tests;

public class ParserTests
{
	[Fact]
	public void Parser_should_parse_a_literal_as_an_invocation()
	{
		var command = "this";
		var lexer = new Lexer(command);
		var parser = new Parser(lexer);
		var script = parser.Parse();
		
		var invoc = script.Root as Invocation;
		Assert.NotNull(invoc);
		Assert.Single(invoc.Command.Segments);
		Assert.Equal("this", invoc.Command.Segments.First().Token.Literal);
	}
	
	[Fact]
	public void Parser_should_parse_a_command_path_as_an_invocation()
	{
		var command = "this.is.my.command";
		var lexer = new Lexer(command);
		var parser = new Parser(lexer);
		var script = parser.Parse();
		
		var invoc = script.Root as Invocation;
		Assert.NotNull(invoc);
		Assert.Equal(4, invoc.Command.Segments.Count());
		
		var segments = invoc.Command.Segments.ToList();
		Assert.Equal("this", segments[0].Token.Literal);
		Assert.Equal("is", segments[1].Token.Literal);
		Assert.Equal("my", segments[2].Token.Literal);
		Assert.Equal("command", segments[3].Token.Literal);
	}

	[Fact]
	public void Parser_should_parse_invocations_with_params()
	{
		var command = "cool.command @param1 param2 ( @param3 ( @param4 ) )";
		var lexer = new Lexer(command);
		var parser = new Parser(lexer);
		var script = parser.Parse();
		
		var invoc = script.Root as Invocation;
		Assert.NotNull(invoc);
		
		var segments = invoc.Command.Segments.ToList();
		Assert.Equal("cool", segments[0].Token.Literal);
		Assert.Equal("command", segments[1].Token.Literal);
		
		var pars = invoc.Params?.Params.ToList();
		Assert.NotNull(pars);
		
		var refParam = pars[0] as Ref;
		Assert.NotNull(refParam);
		Assert.Equal(TokenType.At, refParam.Type.Type);
		Assert.Equal("param1", refParam.Lit.Token.Literal);
		
		var litParam = pars[1] as Lit;
		Assert.NotNull(litParam);
		Assert.Equal("param2", litParam.Token.Literal);
		
		var tupleParam = pars[2] as Script.Tuple;
		Assert.NotNull(tupleParam);
		Assert.Equal(2, tupleParam.Members.Count());
		
		var tupleMembers = tupleParam.Members.ToList();
		var ref1 = tupleMembers[0] as Ref;
		Assert.NotNull(ref1);
		Assert.Equal(TokenType.At, ref1.Type.Type);
		Assert.Equal("param3", ref1.Lit.Token.Literal);
		
		var nestedTuple = tupleMembers[1] as Script.Tuple;
		Assert.NotNull(nestedTuple);
		Assert.Equal(1, nestedTuple.Members.Count());
		
		var nestedTupleMembers = nestedTuple.Members.ToList();
		var ref2 = nestedTupleMembers[0] as Ref;
		Assert.NotNull(ref2);
		Assert.Equal(TokenType.At, ref2.Type.Type);
		Assert.Equal("param4", ref2.Lit.Token.Literal);
	}

	[Theory]
	[InlineData("@test", TokenType.At, "test")]
	[InlineData("$test", TokenType.Dollar, "test")]
	[InlineData("#test", TokenType.Pound, "test")]
	public void Parser_should_parse_references(string command, TokenType expType, string expLit)
	{
		var lexer = new Lexer(command);
		var parser = new Parser(lexer);
		var script = parser.Parse();
		
		var r = script.Root as Ref;
		Assert.NotNull(r);
		Assert.Equal(expType, r.Type.Type);
		Assert.Equal(expLit, r.Lit.Token.Literal);
	}

	[Theory]
	[InlineData("var test = bla", TokenType.Var, "test", "bla")]
	[InlineData("secret test = bla", TokenType.Secret, "test", "bla")]
	[InlineData("env test = bla", TokenType.Env, "test", "bla")]
	public void Parser_should_parse_new_assignments(string command, TokenType expType, string expLLit, string expRLit)
	{
		var lexer = new Lexer(command);
		var parser = new Parser(lexer);
		var script = parser.Parse();
		
		var ass = script.Root as Ass;
		Assert.NotNull(ass);
		Assert.Equal(expType, ass.Token.Type);

		var lhs = ass.Lhs as Lit;
		var rhs = ass.Rhs as Lit;
		Assert.NotNull(lhs);
		Assert.NotNull(rhs);
		
		Assert.Equal(expLLit, lhs.Token.Literal);
		Assert.Equal(expRLit, rhs.Token.Literal);
	}
}
