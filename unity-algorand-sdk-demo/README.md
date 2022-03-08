# Unity Algorand SDK

> [!Important]
> This package has not been audited and isn't suitable for production use.

This is a demo game showing off some of the basic features for managing assets,
accounts, and transactions using this SDK.

## Install Unity

To get started, install [Unity Hub](https://unity3d.com/get-unity/download) and the latest 2020.3 LTS version.

Clone this branch with
```bash
> git clone -b demo https://github.com/CareBoo/unity-algorand-sdk
```
then open the project directory with Unity. The correct version of the sdk should install
as a package.

## Setup Algod

You'll need a node to connect to the Algorand `testnet`. The easiest way is using
[the Algorand Sandbox](https://github.com/algorand/sandbox).

Once your node is set up and caught up to the latest block, you'll want to double check that
the Algod client has the correct address and token parameters for the node. In the
`Assets/Algorand/Entities` directory, click on the `Algod` asset to see its `Address` and `Token`
settings. Modify them if they need to be changed. This will be the client the game uses to connect to
the algod node.
![check_algod](https://user-images.githubusercontent.com/5115126/139360476-94c2566a-9e2a-4238-b072-1c5bdb3f538a.gif)

## Setup an account

In the `Assets/Algorand/Entities` directory, there should be an account called `AssetCreator`. Select this account,
open the context menu, and select `Randomize`. This will generate a random account for you to use. Click the
context menu again, and select `LogAccountInfo`. This will log the account private key, address, and amount of algo to
the console in the editor.

Copy the address that was printed to the editor. Head to
the [testnet Algorand Dispenser](https://bank.testnet.algorand.network/)
and dispense funds to the `AssetCreator` account. Now when you select `LogAccountInfo`, you should see that your
account balance has been updated with Algo.

Repeat the above steps for the `PlayerAccount` asset in `Assets/Algorand/Entities`. Generate a new account, copy the
address, and fund the account.


https://user-images.githubusercontent.com/5115126/139360496-b18f13e6-884f-4ef9-b563-0b9fdf53f701.mp4


## Create the smart assets

Click on the `GameCurrency` in `Assets/Algorand/Entities` and open the context menu. Now click on `GetOrCreateAsset`. After a few seconds, the `Index` should be updated to the asset id. You've just created a new smart asset on the testnet.

Repeat for the `SniperRifle` entity in `Assets/Algorand/Entities`.


https://user-images.githubusercontent.com/5115126/139360516-5a9b6964-346c-4a64-a928-8f5f388a2acf.mp4


## Start the game

After creating and funding the `AssetCreator`, `PlayerAccount` accounts, and creating the `GameCurrency`, and `SniperRifle` assets, you are ready to start playing the game.

Double click the scene `Assets/FPS/Scenes/IntroMenu`. Press the **Play** button, and the game should start. In the bottom
left corner you should see your player account's token and algo balance. If you click on the `Weapon Store`, you should
see that there are 100 Sniper Rifles available to purchase. Unfortunately, you do not have enough tokens to purchase the
sniper rifle just yet, so lets earn a token.


https://user-images.githubusercontent.com/5115126/139360794-582f22a8-652a-4776-a271-fe1d74d2a745.mp4



https://user-images.githubusercontent.com/5115126/139360563-0cf8663b-c726-4841-888f-211e79afc842.mp4


## Earn your first token

Press the `PLAY` button in the menu. The `MainScene` should load up. Move your character with `w`, `a`, `s`, and `d` keys, and aim with your mouse. There is an objective in the top left of the screen; complete it to earn a token!


https://user-images.githubusercontent.com/5115126/139360587-2b33f6f8-c68e-4bed-84a3-4dec97cf75cb.mp4


https://user-images.githubusercontent.com/5115126/139360894-63727908-eed3-47e4-bd9b-be804501a984.mp4



## Buy a sniper rifle

Once completing the objective, a `WinScene` should appear. Go back to the main menu to view the game tokens that we won.
You should have earned 1 game token for completing the objectives. Click on the `Weapon Store` again and press `BUY`
button. After a few seconds, you should see the weapon count decrease in the sniper rifles for sale, but increase in the
sniper rifles you own. You just completed an atomic transfer.


https://user-images.githubusercontent.com/5115126/139360988-230193ad-a7a2-4570-9f3f-6e6ef14641b5.mp4


Start the game again. Now you should see a floating sniper rifle when you start the game. Run into the sniper rifle and you should see it pop up into your inventory in the bottom left of the screen. Press the `2` key to activate the sniper rifle.


https://user-images.githubusercontent.com/5115126/139361075-cfa29b39-de37-4b8a-bdaa-8e71a01ed52f.mp4

