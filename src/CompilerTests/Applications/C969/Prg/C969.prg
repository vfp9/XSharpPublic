// 969. Nested WITH statements produce a bogus compiler error #1991
// https://github.com/X-Sharp/XSharpPublic/issues/1991

FUNCTION Start( ) AS VOID
	LOCAL c := "abc" AS STRING
	WITH c
		? .Length
		WITH .Length // error XS0841: Cannot use local variable 'Xs$WithVar$C969_93_8_2' before it is declared
			? .ToString()
		END WITH
	END WITH
RETURN
