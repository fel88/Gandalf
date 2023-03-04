# Gandalf

Modify, build and create pull requests for your C# projects with Telegram Bot.


![image](https://user-images.githubusercontent.com/15663687/222923525-5848ce89-60a9-4404-91b3-5383e259448c.png)

Commands:
```
ls
cd <folder>
cd..
build # build current directory solution with msbuild
git <any git-bash command>
imgcat <file> # get image
enter <file>  # enter file mode
exit # exit file mode
```

directory mode:
```
cat <file>
patch <filename>\n<code>
parse <file>
```
file mode:
```
func <name> <start> <qty>  [-nolines] # display specific function
cat <lines qty> 
cat <start line>  <lines qty>
patch <func name>\n<code>
```
