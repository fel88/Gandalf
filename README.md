# Gandalf

Modify, build and create pull requests for your C# projects with Telegram Bot.

git-bash / github cli / cmd

QR code parser

Roslyn based parsing

![image](https://user-images.githubusercontent.com/15663687/222923525-5848ce89-60a9-4404-91b3-5383e259448c.png)

For Linux:
```
bash git <any git commands>
bash gh <any github cli commands>
bash <any bash commands>
```

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


How to apply git patch remotely:
```
1. git add -A
2. git diff --staged -w > changes.patch
3. Zip changes.patch to archive
4. Open QR split (https://github.com/fel88/QRSplit) and make QR code
5. Photo QR code and send to gandalf telegram bot
6. Unzip file (bash 7z x <file_name>)
7. apply changes to target repository: git apply --reject --ignore-space-change --ignore-whitespace changes.patch
8. remove changes.zip and changes.patch (bash rm changes.*)
9. bash git add .
10. bash git commit -m '<text>'
11. bash git push
12. bash gh pr create --title '<title>' --body '<body>'
13. merge PR with main account
14. sync repo: bash gh repo sync <gandalf-account-name>/<repo-name>
15. bash git pull
```
