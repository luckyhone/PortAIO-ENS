import os 

path = 'C:/Users/eweew/Documents/LyrdumAIO-main/PortAIO/Resources/minimap'

file = os.listdir(path)

arch = open("newtxt.txt","w")

string = ""

for f in file:
    fnew = f.replace("png","")
    fnew2 = fnew.replace(".","") #clean
    string +=(f'<data name="{fnew2}" type="System.Resources.ResXFileRef, System.Windows.Forms">\n')
    string +=(f'<value>..\Resources\minimap\{fnew2}.png;System.Drawing.Bitmap, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a</value>\n')
    string += ('</data>\n')
print(string)
arch.writelines(string)