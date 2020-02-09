000100 Id Division.                                                     
000110 Program-Id. BotBeer. 
000115* http://99-bottles-of-beer.net/language-cobol-908.html                                            
000120 Data Division.                                                   
000130 Working-Storage Section.                                         
000140 01  strings.                                                     
000150     05  buffer                      pic x(80).                   
000160     05  bb1 value spaces            pic x(15).                   
000170     05  bb2 value 'bottles of beer' pic x(15).                   
000180     05  bb3 value 'on the wall'     pic x(11).                   
000190     05  bb4 value 'Take one down, pass it around, '  pic x(31).  
000200     05  bb6.                                                     
000210         10                          pic x(010) value spaces.     
000220         10                          pic x(130) value 'one       two       three     four      five      six       seven 
000240-    '    eight     nine      ten       eleven    twelve    thirteen  '.  
000250         10                          pic x(060) value 'fourteen  fifteen   sixteen   seventeen eighteen  nineteen  '.     
000270     05  redefines bb6.                                           
000280         10  bb7 occurs 20           pic x(10).                   
000290     05  value '                twenty  thirty  forty   fifty   sixty   s
000300-              'eventy eighty  ninety    '.                            
000310         10  bb8 occurs 10          pic x(08).                    
000320 01  integers                       binary.                       
000330     05  i                          pic s9(3).                    
000340     05  j                          pic s9(3).                    
000350     05  k                          pic s9(3).                    
000360     05  l                          pic s9(3).                    
000370 Procedure Division.                                              
000380 0.                                                               
000390     perform varying i from 99 by -1 until i = 1                  
000400        move spaces to buffer bb1                                 
000410        move 1 to j
000420        divide i by 10 giving k remainder l                       
000430        string bb8(k + 1) delimited space into bb1 pointer j   
000431*This is a test line:
000440        if j > 1   
000450           then move bb7(l + 1) to bb1(j + 1:)                    
000460           else move bb7(i + 1) to bb1                            
000470        end-if                                                    
000473        STRING bb1 DELIMITED '  ' INTO bb1
000480        move 1 to j                                               
000490        string bb1 ' ' bb2 ' ' bb3 delimited '  ' into buffer pointer j                                  
000510        if i < 99                                                 
000520           move '!' to buffer(j:)                                 
000530           display bb4 buffer                                    
000540           display ' '                                            
000550        end-if                                                    
000560        string ', ' bb1 ' ' bb2 '!' delimited '  ' into buffer(j:) pointer j                   
000580        display function upper-case(buffer(1:1)) buffer(2:)  
000590     end-perform                                                  
000600     display bb4 'one bottle of beer on the wall!'                
000610     display ' '                                                  
000620     display 'One bottle of beer on the wall, one bottle of beer!'
000630     display 'Take that down, pass it around, '                   
000640             'no more bottles of beer on the wall!'               
000650     display ' '                                                  
000660     display 'No bottle of beer on the wall, '                
000670             'no more bottles of beer!'                       
000680     display 'Go to the store and buy some more, '            
000690             'ninety nine bottles of beer on the wall!'       
000700     stop run                                                 
000710     .                                                        
000720 end program BotBeer.
