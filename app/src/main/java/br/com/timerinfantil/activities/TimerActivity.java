package br.com.timerinfantil.activities;

import android.app.Activity;
import android.media.AudioManager;
import android.media.ToneGenerator;
import android.os.Bundle;
import android.os.CountDownTimer;
import android.os.Handler;
import android.os.Looper;
import android.view.View;
import android.widget.Button;
import android.widget.ProgressBar;
import android.widget.TextView;

import br.com.timerinfantil.R;

public class TimerActivity extends Activity {

    private TextView activityNameText;
    private TextView timeText;
    private TextView feedbackText;
    private ProgressBar progressBar;
    private Button pauseButton;

    private CountDownTimer countDownTimer;
    private long totalMillis;
    private long remainingMillis;
    private boolean isPaused;
    private boolean isFreePlay;
    private boolean rewardSaved;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_timer);

        bindViews();
        readActivityData();
        setupButtons();
        updateScreen();
        startTimer();
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        stopTimer();
    }

    private void bindViews() {
        activityNameText = findViewById(R.id.textTimerActivityName);
        timeText = findViewById(R.id.textTimerTime);
        feedbackText = findViewById(R.id.textTimerFeedback);
        progressBar = findViewById(R.id.progressTimer);
        pauseButton = findViewById(R.id.buttonPause);
    }

    // Reaproveita esta tela para todas as atividades escolhidas na tela inicial.
    private void readActivityData() {
        String activityName = getIntent().getStringExtra(MainActivity.EXTRA_ACTIVITY_NAME);
        int durationSeconds = getIntent().getIntExtra(MainActivity.EXTRA_DURATION_SECONDS, 60);

        if (activityName == null) {
            activityName = getString(R.string.activity_default);
        }

        isFreePlay = getIntent().getBooleanExtra(MainActivity.EXTRA_IS_FREE_PLAY, false);
        totalMillis = durationSeconds * 1000L;
        remainingMillis = totalMillis;

        activityNameText.setText(activityName);
        progressBar.setMax(100);
    }

    private void setupButtons() {
        pauseButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                toggleTimer();
            }
        });

        findViewById(R.id.buttonBackHome).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                finish();
            }
        });
    }

    // CountDownTimer e nativo do Android e consome pouco para este fluxo simples.
    private void startTimer() {
        isPaused = false;
        pauseButton.setText("Pausar");
        feedbackText.setText(R.string.timer_feedback_start);

        countDownTimer = new CountDownTimer(remainingMillis, 1000) {
            @Override
            public void onTick(long millisUntilFinished) {
                remainingMillis = millisUntilFinished;
                updateScreen();
            }

            @Override
            public void onFinish() {
                remainingMillis = 0;
                updateScreen();
                finishActivityTimer();
            }
        }.start();
    }

    private void toggleTimer() {
        if (remainingMillis <= 0) {
            return;
        }

        if (isPaused) {
            startTimer();
        } else {
            stopTimer();
            isPaused = true;
            pauseButton.setText("Continuar");
            feedbackText.setText(R.string.feedback_pause);
        }
    }

    private void stopTimer() {
        if (countDownTimer != null) {
            countDownTimer.cancel();
            countDownTimer = null;
        }
    }

    private void updateScreen() {
        int seconds = (int) (remainingMillis / 1000);
        int minutes = seconds / 60;
        int secondsOnly = seconds % 60;

        timeText.setText(String.format("%02d:%02d", minutes, secondsOnly));

        int progress = 100;
        if (totalMillis > 0) {
            long elapsedMillis = totalMillis - remainingMillis;
            progress = (int) ((elapsedMillis * 100) / totalMillis);
        }
        progressBar.setProgress(progress);
    }

    // Brincadeira livre recebe feedback leve, sem cobranca nem competicao.
    private void finishActivityTimer() {
        stopTimer();
        pauseButton.setEnabled(false);
        playSoftAlarm();

        if (isFreePlay) {
            feedbackText.setText(R.string.feedback_free_play);
        } else {
            feedbackText.setText(R.string.feedback_done);
            saveRewardStar();
        }
    }

    // Volume baixo sinaliza o fim do timer sem assustar a crianca.
    private void playSoftAlarm() {
        final ToneGenerator toneGenerator = new ToneGenerator(AudioManager.STREAM_MUSIC, 45);
        toneGenerator.startTone(ToneGenerator.TONE_PROP_BEEP2, 500);

        new Handler(Looper.getMainLooper()).postDelayed(new Runnable() {
            @Override
            public void run() {
                toneGenerator.release();
            }
        }, 700);
    }

    // Uma estrela por conclusao e suficiente para reforco positivo no MVP.
    private void saveRewardStar() {
        if (rewardSaved) {
            return;
        }

        int stars = getSharedPreferences("rewards", MODE_PRIVATE).getInt("stars", 0);
        getSharedPreferences("rewards", MODE_PRIVATE)
                .edit()
                .putInt("stars", stars + 1)
                .apply();
        rewardSaved = true;
    }
}
