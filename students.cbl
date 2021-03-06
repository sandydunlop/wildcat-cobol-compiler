000010 IDENTIFICATION DIVISION.
000020 PROGRAM-ID.   FILE-TEST-PROG.
000030 AUTHOR.       SANDY DUNLOP.
000040*http://www.csis.ul.ie/COBOL/Course/SequentialFiles1.htm
000050 
000060 ENVIRONMENT DIVISION.
000070 INPUT-OUTPUT SECTION.
000080 FILE-CONTROL.
000090     SELECT StudentFile ASSIGN TO "STUDENTS.DAT"
000100 	       ORGANIZATION IS LINE SEQUENTIAL.
000110 
000120 DATA DIVISION.
000130 FILE SECTION.
000140 FD StudentFile.
000150 01 StudentRec.
000160    88  EndOfStudentFile  VALUE HIGH-VALUES.
000170    02  StudentId         PIC 9(7).
000180    02  StudentName.
000190        03 Surname        PIC X(8).
000200        03 Initials       PIC XX.
000210    02  DateOfBirth.
000220        03 YOBirth        PIC 9(4).
000230        03 MOBirth        PIC 9(2).
000240        03 DOBirth        PIC 9(2).
000250    02  CourseCode        PIC X(4).
000260    02  Gender            PIC X.
000270 
000280 PROCEDURE DIVISION.
000290 MAIN-PARAGRAPH.
000300     OPEN OUTPUT StudentFile
000310     DISPLAY "Enter student details using template below."
000320     DISPLAY "Enter no data to end"
000330 
000340     PERFORM GetStudentRecord
000350     PERFORM UNTIL StudentRec = SPACES
000360        WRITE StudentRec
000370        PERFORM GetStudentRecord
000380     END-PERFORM
000390     CLOSE StudentFile
000400     OPEN INPUT StudentFile.
000410     READ StudentFile
000420          AT END
000430              SET EndOfStudentFile TO TRUE
000440     END-READ
000450     PERFORM UNTIL EndOfStudentFile
000460        DISPLAY StudentId SPACE StudentName SPACE CourseCode
000470        READ StudentFile
000480             AT END SET EndOfStudentFile TO TRUE
000490        END-READ
000500     END-PERFORM
000510     CLOSE StudentFile
000520     STOP RUN.
000530
000540 GetStudentRecord.
000550     DISPLAY "NNNNNNNSSSSSSSSIIYYYYMMDDCCCCG"
000560     ACCEPT  StudentRec.
