package br.com.timerinfantil.activities;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.TextView;

import br.com.timerinfantil.R;

public class MainActivity extends Activity {

    public static final String EXTRA_ACTIVITY_NAME = "activity_name";
    public static final String EXTRA_DURATION_SECONDS = "duration_seconds";
    public static final String EXTRA_IS_FREE_PLAY = "is_free_play";

    private static final int TWO_MINUTES = 2 * 60;
    private static final int FIVE_MINUTES = 5 * 60;
    private static final int TEN_MINUTES = 10 * 60;
    private static final int FIFTEEN_MINUTES = 15 * 60;
    private static final int TWENTY_MINUTES = 20 * 60;
    private static final int THIRTY_MINUTES = 30 * 60;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        showSavedStars();
        setupActivityButtons();
    }

    @Override
    protected void onResume() {
        super.onResume();
        showSavedStars();
    }

    // Cada botao abre o mesmo timer com nome, duracao e regra emocional da atividade.
    private void setupActivityButtons() {
        findViewById(R.id.buttonBrushTeeth).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                openTimer("Escovar os dentes", TWO_MINUTES, false);
            }
        });

        findViewById(R.id.buttonSchool).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                openTimer("Ir para escola", TEN_MINUTES, false);
            }
        });

        findViewById(R.id.buttonCleanRoom).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                openTimer("Arrumar quarto", FIFTEEN_MINUTES, false);
            }
        });

        findViewById(R.id.buttonHomework).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                openTimer("Dever de casa", TWENTY_MINUTES, false);
            }
        });

        findViewById(R.id.buttonReading).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                openTimer("Leitura", TEN_MINUTES, false);
            }
        });

        findViewById(R.id.buttonSleep).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                openTimer("Dormir", FIVE_MINUTES, false);
            }
        });

        findViewById(R.id.buttonFreePlay).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                openTimer("Brincadeira livre", THIRTY_MINUTES, true);
            }
        });
    }

    // Intent com extras mantem a navegacao simples, sem bibliotecas ou arquitetura pesada.
    private void openTimer(String activityName, int durationSeconds, boolean isFreePlay) {
        Intent intent = new Intent(this, TimerActivity.class);
        intent.putExtra(EXTRA_ACTIVITY_NAME, activityName);
        intent.putExtra(EXTRA_DURATION_SECONDS, durationSeconds);
        intent.putExtra(EXTRA_IS_FREE_PLAY, isFreePlay);
        startActivity(intent);
    }

    // SharedPreferences guarda a recompensa local do MVP sem usar banco de dados.
    private void showSavedStars() {
        int stars = getSharedPreferences("rewards", MODE_PRIVATE).getInt("stars", 0);
        TextView starsText = findViewById(R.id.textStars);
        starsText.setText("Estrelas: " + stars);
    }
}
