﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    //GameObjects
    public GameObject board;
    public GameObject[] cops = new GameObject[2];
    public GameObject robber;
    public Text rounds;
    public Text finalMessage;
    public Button playAgainButton;

    //Otras variables
    Tile[] tiles = new Tile[Constants.NumTiles];
    private int roundCount = 0;
    private int state;
    private int clickedTile = -1;
    private int clickedCop = 0;
                    
    void Start()
    {        
        InitTiles();
        InitAdjacencyLists();
        state = Constants.Init;
    }
        
    //Rellenamos el array de casillas y posicionamos las fichas
    void InitTiles()
    {
        for (int fil = 0; fil < Constants.TilesPerRow; fil++)
        {
            GameObject rowchild = board.transform.GetChild(fil).gameObject;            

            for (int col = 0; col < Constants.TilesPerRow; col++)
            {
                GameObject tilechild = rowchild.transform.GetChild(col).gameObject;                
                tiles[fil * Constants.TilesPerRow + col] = tilechild.GetComponent<Tile>();                         
            }
        }
                
        cops[0].GetComponent<CopMove>().currentTile=Constants.InitialCop0;
        cops[1].GetComponent<CopMove>().currentTile=Constants.InitialCop1;
        robber.GetComponent<RobberMove>().currentTile=Constants.InitialRobber;           
    }

    public void InitAdjacencyLists()
    {
        //Matriz de adyacencia
        int[,] matriu = new int[Constants.NumTiles, Constants.NumTiles];

        //TODO: Inicializar matriz a 0's
        for(int i = 0; i < Constants.NumTiles; i++){
            for (int j = 0; j < Constants.NumTiles; j++){
                matriu[i, j] = 0;
            }
        }
        //TODO: Para cada posición, rellenar con 1's las casillas adyacentes (arriba, abajo, izquierda y derecha)
        for (int i = 0; i < Constants.NumTiles; ++i)
        {
            // Arriba
            if (i > 7) { 
                matriu[i, i - 8] = 1; 
            }
            // Abajo
            if (i < 56) { 
                matriu[i, i + 8] = 1; 
            }
            // Derecha
            if (((i + 1) % 8) != 0) { 
                matriu[i, i + 1] = 1; 
            }
            // Izquierda
            if (i % 8 != 0) {
                matriu[i, i - 1] = 1; 
            }
        }
        //TODO: Rellenar la lista "adjacency" de cada casilla con los índices de sus casillas adyacentes
        for (int i = 0; i < Constants.NumTiles; ++i)
        {
            for (int j = 0; j < Constants.NumTiles; ++j)
            {
                if (matriu[i, j] == 1)
                {
                    tiles[i].adjacency.Add(j);
                }
            }
        }

    }

    //Reseteamos cada casilla: color, padre, distancia y visitada
    public void ResetTiles()
    {        
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }

    public void ClickOnCop(int cop_id)
    {
        switch (state)
        {
            case Constants.Init:
            case Constants.CopSelected:                
                clickedCop = cop_id;
                clickedTile = cops[cop_id].GetComponent<CopMove>().currentTile;
                tiles[clickedTile].current = true;

                ResetTiles();
                FindSelectableTiles(true);

                state = Constants.CopSelected;                
                break;            
        }
    }

    public void ClickOnTile(int t)
    {                     
        clickedTile = t;

        switch (state)
        {            
            case Constants.CopSelected:
                //Si es una casilla roja, nos movemos
                if (tiles[clickedTile].selectable)
                {                  
                    cops[clickedCop].GetComponent<CopMove>().MoveToTile(tiles[clickedTile]);
                    cops[clickedCop].GetComponent<CopMove>().currentTile=tiles[clickedTile].numTile;
                    tiles[clickedTile].current = true;   
                    
                    state = Constants.TileSelected;
                }                
                break;
            case Constants.TileSelected:
                state = Constants.Init;
                break;
            case Constants.RobberTurn:
                state = Constants.Init;
                break;
        }
    }

    public void FinishTurn()
    {
        switch (state)
        {            
            case Constants.TileSelected:
                ResetTiles();

                state = Constants.RobberTurn;
                RobberTurn();
                break;
            case Constants.RobberTurn:                
                ResetTiles();
                IncreaseRoundCount();
                if (roundCount <= Constants.MaxRounds)
                    state = Constants.Init;
                else
                    EndGame(false);
                break;
        }

    }

    public void RobberTurn()
    {
        clickedTile = robber.GetComponent<RobberMove>().currentTile;
        tiles[clickedTile].current = true;
        FindSelectableTiles(false);

        /*TODO: Cambia el código de abajo para hacer lo siguiente
        - Elegimos una casilla aleatoria entre las seleccionables que puede ir el caco
        - Movemos al caco a esa casilla
        - Actualizamos la variable currentTile del caco a la nueva casilla
        */
        List<int> adyLadron = new List<int>();
        for(int i=0; i < Constants.NumTiles; i++)
        {
            if (tiles[i].selectable)
                adyLadron.Add(i);
                
        }
        int random =Random.Range(0, adyLadron.Count);
        robber.GetComponent<RobberMove>().currentTile = adyLadron[random];
        robber.GetComponent<RobberMove>().MoveToTile(tiles[robber.GetComponent<RobberMove>().currentTile]);
    }

    public void EndGame(bool end)
    {
        if(end)
            finalMessage.text = "You Win!";
        else
            finalMessage.text = "You Lose!";
        playAgainButton.interactable = true;
        state = Constants.End;
    }

    public void PlayAgain()
    {
        cops[0].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop0]);
        cops[1].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop1]);
        robber.GetComponent<RobberMove>().Restart(tiles[Constants.InitialRobber]);
                
        ResetTiles();

        playAgainButton.interactable = false;
        finalMessage.text = "";
        roundCount = 0;
        rounds.text = "Rounds: ";

        state = Constants.Restarting;
    }

    public void InitGame()
    {
        state = Constants.Init;
         
    }

    public void IncreaseRoundCount()
    {
        roundCount++;
        rounds.text = "Rounds: " + roundCount;
    }

    public void FindSelectableTiles(bool cop)
    {     
        int indexcurrentTile;        

        if (cop==true)
            indexcurrentTile = cops[clickedCop].GetComponent<CopMove>().currentTile;
        else
            indexcurrentTile = robber.GetComponent<RobberMove>().currentTile;

        //La ponemos rosa porque acabamos de hacer un reset
        tiles[indexcurrentTile].current = true;
        tiles[indexcurrentTile].visited = true;

        //Cola para el BFS
        Queue<Tile> nodes = new Queue<Tile>();

        //Indices policias
        List<int> copIndex = new List<int>();
        foreach(GameObject copAux in cops)
        {
            copIndex.Add(copAux.GetComponent<CopMove>().currentTile);
        }
        foreach(int index in tiles[indexcurrentTile].adjacency)
        {
            tiles[index].parent = tiles[indexcurrentTile];
            nodes.Enqueue(tiles[index]);
        }
        //TODO: Implementar BFS. Los nodos seleccionables los ponemos como selectable=true
        //Tendrás que cambiar este código por el BFS
        while(nodes.Count > 0)
        {
            //quita el primer elemento
            Tile auxNodo = nodes.Dequeue();
            //si el primer nodo no está visitado
            if (!auxNodo.visited)
            {
                //cambiamos su estado
                auxNodo.visited = true;
                //diagonales
                auxNodo.distance = auxNodo.parent.distance + 1;
                //No recorre la casilla del poli si no contiene ese nodo
                if (!copIndex.Contains(auxNodo.numTile))
                {
                    foreach (int index in auxNodo.adjacency)
                    {
                        //No se puede ir a las casillas por los polis ni a su casilla
                        if (!tiles[index].visited)
                        {
                            tiles[index].parent = auxNodo;
                            nodes.Enqueue(tiles[index]);
                        }
                    }
                }
                
            }
        }
        foreach(Tile til in tiles)
        {
            //No se puede ir a donde esten los polis, esto incluye a  su propia casilla.
            if(til.distance <= 2 && !copIndex.Contains(til.numTile))
            {
                til.selectable = true;
            }
        }
    }
}
