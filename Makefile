COMPILER=mcs /debug
COBOLC=cobolc.exe
PREFIX=/usr/local
DEBUG=--debug

VERNUM=0.1.16.1

VER=$(VERNUM)-bin
SRC=$(VERNUM)-src
NS=Wildcat.Cobol.Compiler
DOLLARSTAR="$$"*

STRUCTURES= $(NS)/Structure/AST.cs \
			$(NS)/Structure/Program.cs \
			$(NS)/Structure/Division.cs \
			$(NS)/Structure/IdentificationDivision.cs \
			$(NS)/Structure/DataDivision.cs \
			$(NS)/Structure/EnvironmentDivision.cs \
			$(NS)/Structure/ProcedureDivision.cs \
			$(NS)/Structure/WorkingStorageSection.cs \
			$(NS)/Structure/ConfigurationSection.cs \
			$(NS)/Structure/DataDescription.cs \
			$(NS)/Structure/Paragraph.cs \
			$(NS)/Structure/Command.cs \
			$(NS)/Structure/AcceptVerb.cs \
			$(NS)/Structure/DisplayVerb.cs \
			$(NS)/Structure/StringVerb.cs \
			$(NS)/Structure/ExitStatement.cs \
			$(NS)/Structure/Literal.cs \
			$(NS)/Structure/Pic.cs \
			$(NS)/Structure/Source.cs \
			$(NS)/Structure/Identifier.cs \
			$(NS)/Structure/PerformVerb.cs \
			$(NS)/Structure/PerformVaryingPhrase.cs \
			$(NS)/Structure/Condition.cs \
			$(NS)/Structure/RelationCondition.cs \
			$(NS)/Structure/ArithmeticExpression.cs \
			$(NS)/Structure/RelationalOperator.cs \
			$(NS)/Structure/TimesDiv.cs \
			$(NS)/Structure/Power.cs \
			$(NS)/Structure/Basis.cs \
			$(NS)/Structure/IfStatement.cs \
			$(NS)/Structure/MoveStatement.cs \
			$(NS)/Structure/IntrinsicFunction.cs \
			$(NS)/Structure/AddStatement.cs \
			$(NS)/Structure/DivideStatement.cs \
			$(NS)/Structure/Repository.cs \
			$(NS)/Structure/ClassDefinition.cs \
			$(NS)/Structure/SetStatement.cs \
			$(NS)/Structure/InvokeStatement.cs \
			$(NS)/Structure/SubtractStatement.cs \
			$(NS)/Structure/MultiplyStatement.cs \
			$(NS)/Structure/OpenStatement.cs \
			$(NS)/Structure/CloseStatement.cs \
			$(NS)/Structure/ReadStatement.cs \
			$(NS)/Structure/WriteStatement.cs \
			$(NS)/Structure/InputOutputSection.cs \
			$(NS)/Structure/FileControlParagraph.cs \
			$(NS)/Structure/FileControlEntry.cs \
			$(NS)/Structure/IOControlParagraph.cs \
			$(NS)/Structure/IOControlEntry.cs \
			$(NS)/Structure/SelectClause.cs \
			$(NS)/Structure/AssignClause.cs \
			$(NS)/Structure/FileSection.cs \
			$(NS)/Structure/FileAndSortDescriptionEntry.cs \
			$(NS)/Structure/FigurativeConstant.cs \
			$(NS)/Structure/Sentence.cs

ILGEN =     $(NS)/ILGenerator/ILGenerator.cs \
			$(NS)/ILGenerator/Invoke.cs \
			$(NS)/ILGenerator/Condition.cs \
			$(NS)/ILGenerator/IO.cs

ALL=cobolc.exe $(NS).Parser.dll

cobolc.exe: Properties/AssemblyInfo.cs $(NS).Exceptions.dll $(NS)/CompilerDriver.cs $(NS).Parser.dll $(NS).ILGenerator.dll $(NS).ContextualAnalyzer.dll $(NS).References.dll
	$(COMPILER) /out:cobolc.exe $(NS)/CompilerDriver.cs Properties/AssemblyInfo.cs /reference:$(NS).ContextualAnalyzer.dll /reference:$(NS).Parser.dll /reference:$(NS).Exceptions.dll /reference:$(NS).Structure.dll /reference:$(NS).ILGenerator.dll /reference:$(NS).References.dll

$(NS).Structure.dll: $(STRUCTURES)
	$(COMPILER) /t:library /out:$(NS).Structure.dll $(STRUCTURES)

$(NS).References.dll: $(NS)/References/ReferenceManager.cs
	$(COMPILER) /t:library /out:$(NS).References.dll $(NS)/References/ReferenceManager.cs

$(NS).ContextualAnalyzer.dll: $(NS)/ContextualAnalyzer/ContextualAnalyzer.cs $(NS).Structure.dll $(NS).Exceptions.dll $(NS).References.dll
	$(COMPILER) /t:library /out:$(NS).ContextualAnalyzer.dll $(NS)/ContextualAnalyzer/ContextualAnalyzer.cs /reference:$(NS).Structure.dll /reference:$(NS).Exceptions.dll /reference:$(NS).References.dll

$(NS).Exceptions.dll: $(NS)/Exceptions/CompilerException.cs
	$(COMPILER) /t:library /out:$(NS).Exceptions.dll $(NS)/Exceptions/CompilerException.cs

$(NS).ILGenerator.dll: $(NS).Structure.dll $(ILGEN) $(NS).References.dll
	$(COMPILER) /t:library /out:$(NS).ILGenerator.dll /reference:$(NS).Structure.dll /reference:$(NS).Exceptions.dll /reference:$(NS).References.dll $(ILGEN)

$(NS).Parser.dll: $(NS).Exceptions.dll $(NS).Structure.dll $(NS)/Parser/Parser.cs $(NS)/Parser/Symbol.cs $(NS)/Parser/Symbols.cs $(NS)/Parser/Tokenizer.cs
	$(COMPILER) /t:library /out:$(NS).Parser.dll $(NS)/Parser/Parser.cs $(NS)/Parser/Symbol.cs  $(NS)/Parser/Symbols.cs $(NS)/Parser/Tokenizer.cs /reference:$(NS).Structure.dll /reference:$(NS).Exceptions.dll

Properties/AssemblyInfo.cs: Makefile
	cp Properties/AssemblyInfo.cs tempinfo.cs
	sed 's/AssemblyVersion(".*")/AssemblyVersion("$(VERNUM).0")/' tempinfo.cs > Properties/AssemblyInfo.cs
	cp Properties/AssemblyInfo.cs tempinfo.cs
	sed 's/AssemblyFileVersion(".*")/AssemblyFileVersion("$(VERNUM).0")/' tempinfo.cs > Properties/AssemblyInfo.cs
	rm tempinfo.cs

binzip:
	rm -rf WildcatCobol-$(VER)
	mkdir WildcatCobol-$(VER)
	cp $(NS).Parser.dll WildcatCobol-$(VER)
	cp $(NS).Structure.dll WildcatCobol-$(VER)
	cp $(NS).ContextualAnalyzer.dll WildcatCobol-$(VER)
	cp $(NS).Exceptions.dll WildcatCobol-$(VER)
	cp $(NS).ILGenerator.dll WildcatCobol-$(VER)
	cp $(NS).Parser.dll WildcatCobol-$(VER)
	cp $(NS).References.dll WildcatCobol-$(VER)
	cp cobolc.exe WildcatCobol-$(VER)
	cp hello.cbl WildcatCobol-$(VER)
	cp hellonet.cbl WildcatCobol-$(VER)
	cp tests.cbl WildcatCobol-$(VER)
	cp bottles.cbl WildcatCobol-$(VER)
	cp hanoi.cbl WildcatCobol-$(VER)
	cp fileopen.cbl WildcatCobol-$(VER)
	cp STUDENTS.DAT WildcatCobol-$(VER)
	cp README.text WildcatCobol-$(VER)
	cp LICENSE.text WildcatCobol-$(VER)
	cp Makefile WildcatCobol-$(VER)
	rm -rf WildcatCobol-$(VER).zip
	zip -r WildcatCobol-$(VER).zip WildcatCobol-$(VER)
	rm -rf WildcatCobol-$(VER)
	ls -lh WildcatCobol-$(VER).zip
	
srczip:
	rm -rf WildcatCobol-$(SRC)
	mkdir WildcatCobol-$(SRC)
	find ./Wildcat.Cobol.Compiler -name obj -exec rm -rf {} \;
	find ./Wildcat.Cobol.Compiler -name bin -exec rm -rf {} \;
	cp -r Wildcat.Cobol.Compiler WildcatCobol-$(SRC)
	cp -r Properties WildcatCobol-$(SRC)
	cp Makefile WildcatCobol-$(SRC)
	cp WildcatCobol.csproj WildcatCobol-$(SRC)
	cp WildcatCobol.sln WildcatCobol-$(SRC)
	cp hello.cbl WildcatCobol-$(SRC)
	cp hellonet.cbl WildcatCobol-$(SRC)
	cp tests.cbl WildcatCobol-$(SRC)
	cp bottles.cbl WildcatCobol-$(SRC)
	cp hanoi.cbl WildcatCobol-$(SRC)
	cp fileopen.cbl WildcatCobol-$(SRC)
	cp STUDENTS.DAT WildcatCobol-$(SRC)
	cp README.text WildcatCobol-$(SRC)
	cp LICENSE.text WildcatCobol-$(SRC)
	rm -rf WildcatCobol-$(SRC).zip
	zip -r WildcatCobol-$(SRC).zip WildcatCobol-$(SRC)
	rm -rf WildcatCobol-$(SRC)
	ls -lh WildcatCobol-$(SRC).zip

dist: binzip srczip

install:
	@if [ -e "$(PREFIX)/lib/cobolc" ]; then\
		true;\
	else\
		mkdir -p $(PREFIX)/lib/cobolc;\
	fi
	cp *.dll *.exe $(PREFIX)/lib/cobolc/
	@echo "Creating $(PREFIX)/bin/cobolc"
	@echo "#!/bin/sh" > $(PREFIX)/bin/cobolc
	@echo "mono $(DEBUG) $(PREFIX)/lib/cobolc/cobolc.exe " ${DOLLARSTAR} >> $(PREFIX)/bin/cobolc
	@chmod 0755 $(PREFIX)/bin/cobolc

test:
	cp /usr/local/NUnit-2/bin/nunit.framework.dll .
	mono --debug $(COBOLC) /reference:nunit.framework.dll tests.cbl
	nunit-console /labels tests.exe

clean:
	rm -f ./*.dll
	rm -f ./*.exe
