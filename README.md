# Gandalf

Modify, build and create pull requests for your C# projects with Telegram Bot.

git-bash / github cli / cmd

Roslyn based parsing

![image](https://user-images.githubusercontent.com/15663687/222923525-5848ce89-60a9-4404-91b3-5383e259448c.png)

Commands:
```
ls # show files/directories list
cd <folder> # change directory
cd..
build # build current directory solution with msbuild
git <any git-bash command>
gh <any github cli command>
cmd <any cmd command>
imgcat <file> # get image
enter <file>  # enter file mode
exit # exit file mode
```

directory mode:
```
cat <file> # display text file
patch <filename>\n<code> # rewrite file
parse <file> # get list of functions
```
file mode:
```
func <name> <start> <qty>  [-nolines] # display specific function
cat <lines qty> 
cat <start line>  <lines qty> # display lines of file
patch <func name>\n<code> # replace specific function code
parse <lines qty> # show functions list
parse <start line> <lines qty> # show functions list
insert <line>\n<code> #insert line
delete <line>\n<code> # delete line
replace <line>\n<code> # replace line
```
